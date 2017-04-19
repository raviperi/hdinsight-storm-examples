package com.microsoft.hdinsight.storm.hdfs.bolt;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Date;
import java.util.EnumSet;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FSDataOutputStream;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.hdfs.client.HdfsDataOutputStream;
import org.apache.hadoop.hdfs.client.HdfsDataOutputStream.SyncFlag;
import org.apache.storm.hdfs.bolt.format.FileNameFormat;
import org.apache.storm.hdfs.bolt.format.RecordFormat;
import org.apache.storm.hdfs.bolt.rotation.FileRotationPolicy;
import org.apache.storm.hdfs.bolt.rotation.TimedRotationPolicy;
import org.apache.storm.hdfs.bolt.sync.SyncPolicy;
import org.apache.storm.hdfs.common.AbstractHDFSWriter;
import org.apache.storm.hdfs.common.rotation.RotationAction;
import org.apache.storm.hdfs.common.security.HdfsSecurityUtil;
import org.apache.storm.task.OutputCollector;
import org.apache.storm.task.TopologyContext;
import org.apache.storm.topology.OutputFieldsDeclarer;
import org.apache.storm.topology.base.BaseRichBolt;
import org.apache.storm.tuple.Tuple;
import org.apache.storm.utils.TupleUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.esotericsoftware.minlog.Log;
import com.microsoft.hdinsight.storm.hdfs.bolt.sync.AzureSizeSyncPolicy;

/**
 * Reference bolt implementation to use Azure Data Lake Storage backend.
 * 
 * ADL bolt relies on ADL client that buffers data (4MB by default) before
 * persisting the writes on the server.
 * 
 * Data larger than 4MB is broken into chunks, and sent over write as multiple
 * writes. Given the nature of network calls, failures in writes (even with
 * retries) may result in data loss/corruption.
 * 
 * This bolt implementation attempts to mitigate the data loss by 1. batching up
 * tuples (in the ADL client's write buffer) 2. Issuing a sync call any time the
 * buffer size is met (as dictated by the Sync policy) 3. ACK'ing or FAIL'ing
 * the tuples only on successful/failed sync operations.
 * 
 * This batched mode operation ensures atomic writes for tuples, by making sure
 * they are not broken into chunks, and ACK'ing only when the sync succeeds.
 */
public abstract class AbstractAzureHdfsBolt extends BaseRichBolt {
	private static final long serialVersionUID = -7587239895434711879L;
	protected final Logger LOG = LoggerFactory.getLogger(this.getClass());
	private static final Integer DEFAULT_RETRY_COUNT = 3;

	public enum AckPolicy {
		OnSync, OnRotation
	}

	private static final int DEFAULT_TICK_TUPLE_INTERVAL_SECS = 15;

	protected AbstractHDFSWriter writer;
	protected Map<String, Integer> rotationCounterMap = new HashMap<String, Integer>();
	protected ArrayList<RotationAction> rotationActions = new ArrayList<RotationAction>();
	protected OutputCollector collector;
	protected transient FileSystem fs;
	protected SyncPolicy syncPolicy;
	protected FileRotationPolicy rotationPolicy;
	protected FileNameFormat fileNameFormat;
	private RecordFormat format;
	protected AckPolicy ackPolicy = AckPolicy.OnSync;

	protected Integer tickTupleInterval = DEFAULT_TICK_TUPLE_INTERVAL_SECS;

	protected int rotation = 0;
	protected String fsUrl;
	protected String configKey;
	protected transient Object writeLock;
	protected transient Timer rotationTimer; // only used for
												// TimedRotationPolicy
	private transient FSDataOutputStream out;
	protected transient Configuration hdfsConfig;
	private Path currentFile;

	protected Integer fileRetryCount = DEFAULT_RETRY_COUNT;
	private long filerotationOffset = 0L;
	private long offset = 0L;
	private List<Tuple> tupleBuffer;

	public AbstractAzureHdfsBolt(AckPolicy ackPolicy) {
		syncPolicy = new AzureSizeSyncPolicy();
		this.ackPolicy = ackPolicy;
		this.tupleBuffer = new LinkedList<Tuple>();
	}

	public AbstractAzureHdfsBolt withFsUrl(String fsUrl) {
		this.fsUrl = fsUrl;
		return this;
	}

	public AbstractAzureHdfsBolt withConfigKey(String configKey) {
		this.configKey = configKey;
		return this;
	}

	public AbstractAzureHdfsBolt withFileNameFormat(FileNameFormat fileNameFormat) {
		this.fileNameFormat = fileNameFormat;
		return this;
	}

	public AbstractAzureHdfsBolt withRecordFormat(RecordFormat format) {
		this.format = format;
		return this;
	}

	public AbstractAzureHdfsBolt withRotationPolicy(FileRotationPolicy rotationPolicy) {
		this.rotationPolicy = rotationPolicy;
		return this;
	}

	public AbstractAzureHdfsBolt addRotationAction(RotationAction action) {
		this.rotationActions.add(action);
		return this;
	}

	public AbstractAzureHdfsBolt withTickTupleIntervalSeconds(int interval) {
		this.tickTupleInterval = interval;
		return this;
	}

	public AbstractAzureHdfsBolt withFileRetryCount(int fileRetryCount) {
		this.fileRetryCount = fileRetryCount;
		return this;
	}

	protected void rotateOutputFile() throws IOException {
		LOG.info("Rotating output file...");
		long start = System.currentTimeMillis();
		synchronized (this.writeLock) {
			closeOutputFile();
			this.rotation++;

			Path newFile = createOutputFile();
			LOG.info("Performing {} file rotation actions.", this.rotationActions.size());
			for (RotationAction action : this.rotationActions) {
				action.execute(this.fs, this.currentFile);
			}
			this.currentFile = newFile;
		}
		long time = System.currentTimeMillis() - start;
		LOG.info("File rotation took {} ms.", time);
	}

	/**
	 * Marked as final to prevent override. Subclasses should implement the
	 * doPrepare() method.
	 * 
	 * @param conf
	 * @param topologyContext
	 * @param collector
	 */
	public final void prepare(Map conf, TopologyContext topologyContext, OutputCollector collector) {
		this.writeLock = new Object();
		if (this.syncPolicy == null)
			throw new IllegalStateException("SyncPolicy must be specified.");
		if (this.rotationPolicy == null)
			throw new IllegalStateException("RotationPolicy must be specified.");
		if (this.fsUrl == null) {
			throw new IllegalStateException("File system URL must be specified.");
		}

		this.collector = collector;
		this.fileNameFormat.prepare(conf, topologyContext);
		this.hdfsConfig = new Configuration();
		Map<String, Object> map = (Map<String, Object>) conf.get(this.configKey);
		if (map != null) {
			for (String key : map.keySet()) {
				this.hdfsConfig.set(key, String.valueOf(map.get(key)));
			}
		}

		try {
			HdfsSecurityUtil.login(conf, hdfsConfig);
			doPrepare(conf, topologyContext, collector);
			this.currentFile = createOutputFile();

		} catch (Exception e) {
			throw new RuntimeException("Error preparing HdfsBolt: " + e.getMessage(), e);
		}

		if (this.rotationPolicy instanceof TimedRotationPolicy) {
			long interval = ((TimedRotationPolicy) this.rotationPolicy).getInterval();
			this.rotationTimer = new Timer(true);
			TimerTask task = new TimerTask() {
				@Override
				public void run() {
					try {
						rotateOutputFile();
					} catch (IOException e) {
						LOG.warn("IOException during scheduled file rotation.", e);
					}
				}
			};
			this.rotationTimer.scheduleAtFixedRate(task, interval, interval);
		}
	}

	public void execute(Tuple tuple) {
		byte[] bytes = null;
		int tlength = 0;

		synchronized (this.writeLock) {
			boolean forceSync = false;
			if (TupleUtils.isTick(tuple)) {
				LOG.debug("TICK! forcing a file system flush");
				this.collector.ack(tuple);
				forceSync = true;
			}
			bytes = this.format.format(tuple);
			tlength = bytes.length;

			LOG.debug("Checking sync policy {}", new Date().toString());
			if (forceSync || this.syncPolicy.mark(tuple, offset + tlength)) {
				LOG.debug("Sync' data to file system. Offset Len: " + offset);
				boolean success = false;
				int attempts = 0;
				IOException lastException = null;

				while (success == false && attempts < fileRetryCount) {					
					attempts += 1;
					try {
						syncTuples();
						LOG.debug("Data synced to filesystem.");

						if (ackPolicy == AckPolicy.OnSync) {
							ackTupleBatch();
						}

						this.offset = 0;
						syncPolicy.reset();
						success = true;
					} catch (IOException e) {
						LOG.warn("Data could not be synced to filesystem on attempt [{}]", attempts);
						this.collector.reportError(e);
						lastException = e;
					}
				}

				if (success == false) {
					LOG.warn("Data could not be synced to filesystem, failing this batch of tuples");
					failTupleBatch();
					throw new RuntimeException("Sync failed [" + attempts + "] times.", lastException);
				}
			}

			if (this.rotationPolicy.mark(tuple, this.filerotationOffset)) {
				LOG.info("Rotating file: {}", filerotationOffset);
				try {
					rotateOutputFile();
					this.rotationPolicy.reset();
					this.filerotationOffset = 0;
					this.offset = 0;

					if (ackPolicy == AckPolicy.OnRotation || !tupleBuffer.isEmpty()) {
						ackTupleBatch();
					}
				} catch (IOException e) {
					this.collector.reportError(e);
					LOG.error("File could not be rotated at path: {}", this.currentFile);
					if (!tupleBuffer.isEmpty()) {
						failTupleBatch();
					}

					throw new RuntimeException("RotateFile failed for path " + currentFile, e);
				}
			}

			try {
				out.write(bytes);
				this.filerotationOffset += tlength;
				this.offset += tlength;

				tupleBuffer.add(tuple);
				LOG.debug("TupeBuffer size {}, offset: {}, filerotation: {}", tupleBuffer.size(), offset,
						filerotationOffset);

			} catch (IOException e) {
				LOG.error("Failing tuple", e);
				this.collector.fail(tuple);
			}
		}
	}

	protected void closeOutputFile() throws IOException {
		this.out.close();
	}

	protected Path createOutputFile() throws IOException {
		Path filePath = new Path(this.fileNameFormat.getPath(),
				this.fileNameFormat.getName(this.rotation, System.currentTimeMillis()));
		LOG.info("Using output file: " + filePath.getName());

		this.out = this.fs.create(filePath);
		return filePath;
	}

	protected abstract void doPrepare(Map conf, TopologyContext topologyContext, OutputCollector collector)
			throws IOException;

	protected void syncTuples() throws IOException {
		LOG.info("Attempting to sync all data to filesystem");
		long start = System.currentTimeMillis();

		if (this.out instanceof HdfsDataOutputStream) {
			((HdfsDataOutputStream) this.out).hsync(EnumSet.of(SyncFlag.UPDATE_LENGTH));
		} else {
			this.out.hsync();
		}
		long time = System.currentTimeMillis() - start;
		LOG.info("hsync took {} ms.", time);
	}

	private void ackTupleBatch() {
		LOG.info("Acking individual tuples {}", tupleBuffer.size());
		for (Tuple t : tupleBuffer) {
			this.collector.ack(t);
		}
		this.tupleBuffer.clear();
	}

	private void failTupleBatch() {
		LOG.info("Failing individual tuples {} ", tupleBuffer.size());
		for (Tuple t : tupleBuffer) {
			this.collector.ack(t);
		}

		this.tupleBuffer.clear();
	}

	public void declareOutputFields(OutputFieldsDeclarer arg0) {
	}
}

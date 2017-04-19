package com.microsoft.hdinsight.storm.hdfs.bolt.sync;

import org.apache.storm.hdfs.bolt.sync.SyncPolicy;
import org.apache.storm.tuple.Tuple;

/**
 * Size based Sync policy definition to work with Blob/ADLS based stores.
 * 
 * WASB and ADLS stores have a client side buffer of 4MB. A flush is performed
 * in two instances: a. automatically, when the buffer reaches capacity b.
 * client explicitly invokes a flush command in case of ADLS c. client invokes
 * close command on file.
 * 
 * The azure based HDFS writers do not have a record formatter that ensure
 * message boundaries. So, it is a possibility that a given message sent to
 * storage as part of two flush operations. Given the above, it is possible that
 * files written to azure may have incomplete message data written to them.
 * 
 * To avoid above scenarios, the AzureSyncPolicy ensures that a sync call is
 * triggered any time buffer reaches 4MB capacity. To preserve method
 * boundaries, messages that may spill over will trigger sync + reset.
 * 
 * @author raviperi
 */
public class AzureSizeSyncPolicy implements SyncPolicy {
	private static final long serialVersionUID = 1L;

	private static final long BUFFER_MAX_CAPACITY = 4 * 1024 * 1024;

	public boolean mark(Tuple tuple, long offset) {
		if ((offset) > BUFFER_MAX_CAPACITY) {
			return true;
		}

		return false;
	}

	public void reset() {
	}
}

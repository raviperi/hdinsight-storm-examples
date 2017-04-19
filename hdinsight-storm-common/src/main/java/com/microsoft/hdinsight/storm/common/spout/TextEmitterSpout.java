package com.microsoft.hdinsight.storm.common.spout;

import java.util.HashMap;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.apache.storm.shade.org.eclipse.jetty.util.log.Log;
import org.apache.storm.spout.SpoutOutputCollector;
import org.apache.storm.task.TopologyContext;
import org.apache.storm.topology.OutputFieldsDeclarer;
import org.apache.storm.topology.base.BaseRichSpout;
import org.apache.storm.tuple.Fields;
import org.apache.storm.tuple.Values;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class TextEmitterSpout extends BaseRichSpout {
	private static Logger LOG = LoggerFactory.getLogger(TextEmitterSpout.class);
	private static final long serialVersionUID = 1L;
	private String fixedString = "$";
	private Map<Long, String> cache = new HashMap<Long, String>();
	SpoutOutputCollector _collector;
	private long messageId = 0l;

	private String prefix = "";
	private String suffix = "";

	public TextEmitterSpout withPrefix(String prefix) {
		this.prefix = prefix;
		return this;
	}

	public TextEmitterSpout withSuffix(String suffix) {
		this.suffix = suffix;
		return this;
	}

	public TextEmitterSpout withFillerTextLength(int fillerTextLength) {
		fixedString = StringUtils.leftPad("", fillerTextLength, "$");
		return this;
	}

	@Override
	public void ack(Object id) {
		LOG.info("ACK'd tuple: " + id );
		cache.remove(id);
	}

	@Override
	public void fail(Object id) {
		Long msgid = (Long) id;
		if (cache.containsKey(msgid)) {
			LOG.warn("Failed tuple id: " + msgid);
			_collector.emit(new Values(prefix + fixedString + suffix), msgid);
		}
	}

	public void nextTuple() {
		++messageId;
		String msg = prefix + fixedString + suffix;
		_collector.emit(new Values(msg), messageId);
		cache.put(messageId, msg);
	}

	public void declareOutputFields(OutputFieldsDeclarer declarer) {
		declarer.declare(new Fields("message"));
	}

	public void open(Map conf, TopologyContext context, SpoutOutputCollector collector) {
		_collector = collector;
	}
}
package com.microsoft.hdinsight.storm.hdfs.bolt;

import java.io.IOException;
import java.net.URI;
import java.util.Map;

import org.apache.hadoop.fs.FileSystem;
import org.apache.storm.task.OutputCollector;
import org.apache.storm.task.TopologyContext;
import org.apache.storm.topology.OutputFieldsDeclarer;

public class AzureWasbBolt extends AbstractAzureHdfsBolt {
	public AzureWasbBolt() {
		super(AckPolicy.OnRotation);
	}

	@Override
	protected void doPrepare(Map arg0, TopologyContext arg1, OutputCollector arg2) throws IOException {
		LOG.info("Preparing HDFS Bolt...");
		this.fs = FileSystem.get(URI.create(this.fsUrl), hdfsConfig);
	}	
}

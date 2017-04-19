package com.microsoft.hdinsight.storm.hdfs.bolt;

import java.io.IOException;
import java.net.URI;
import java.util.Map;

import org.apache.hadoop.fs.FileSystem;
import org.apache.storm.task.OutputCollector;
import org.apache.storm.task.TopologyContext;

public class AzureAdlsBolt extends AbstractAzureHdfsBolt {

	public AzureAdlsBolt() {
		super(AckPolicy.OnSync);
	}

	@Override
	protected void doPrepare(Map conf, TopologyContext topologyContext, OutputCollector collector) throws IOException {
		LOG.info("Preparing ADLS Bolt...");
		try {
			LOG.info("Type of class HdiAdlFileSystem is" + Class.forName("org.apache.hadoop.fs.adl.HdiAdlFileSystem",
					false, this.getClass().getClassLoader()));
		} catch (ClassNotFoundException e) {
			LOG.error("Cannot load class: org.apache.hadoop.fs.adl.HdiAdlFileSystem");
		}
		this.fs = FileSystem.get(URI.create(this.fsUrl), hdfsConfig);
	}
}

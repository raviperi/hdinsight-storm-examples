package com.microsoft.hdinsight.storm.examples.simple.Wasb;

import org.apache.commons.cli.CommandLine;
import org.apache.commons.cli.CommandLineParser;
import org.apache.commons.cli.GnuParser;
import org.apache.commons.cli.HelpFormatter;
import org.apache.commons.cli.Option;
import org.apache.commons.cli.Options;
import org.apache.commons.cli.ParseException;
import org.apache.storm.Config;
import org.apache.storm.StormSubmitter;
import org.apache.storm.hdfs.bolt.format.DelimitedRecordFormat;
import org.apache.storm.hdfs.bolt.format.FileNameFormat;
import org.apache.storm.hdfs.bolt.rotation.FileSizeRotationPolicy;
import org.apache.storm.hdfs.bolt.rotation.FileSizeRotationPolicy.Units;
import org.apache.storm.topology.TopologyBuilder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.microsoft.hdinsight.storm.common.spout.TextEmitterSpout;
import com.microsoft.hdinsight.storm.hdfs.bolt.AbstractAzureHdfsBolt;
import com.microsoft.hdinsight.storm.hdfs.bolt.AzureWasbBolt;
import com.microsoft.hdinsight.storm.hdfs.bolt.format.HourlyFileNameFormat;

public class SimpleWasbWriterTopology {

	private static final Logger LOG = LoggerFactory.getLogger(SimpleWasbWriterTopology.class);

	public static void main(String[] args) throws Exception {
		Options options = new Options();

		Option topologyNameOption = new Option("n", "topologyName", true,
				"Topology Name Example: wasbPerfTestTopology");
		topologyNameOption.setRequired(true);
		options.addOption(topologyNameOption);

		Option wasbUrlOption = new Option("u", "wasbRepoUrl", true,
				"WASB File repo url. Example: wasb:/// (This uses the default wasb storage account)");
		wasbUrlOption.setRequired(true);
		options.addOption(wasbUrlOption);

		Option wasbFileRootOption = new Option("fr", "wasbFileRoot", true,
				"Root folder for files. Example: \"/simpleWasbTest/\"");
		wasbFileRootOption.setRequired(true);
		options.addOption(wasbFileRootOption);

		Option workersOption = new Option("w", "workers", true,
				"Number of workers. (Usually equals number of worker nodes)");
		workersOption.setRequired(true);
		options.addOption(workersOption);

		Option spoutParallelismOption = new Option("sp", "spoutParallelism", true,
				"Parallelism for Data emitting Spout. Example: 1");
		spoutParallelismOption.setRequired(true);
		options.addOption(spoutParallelismOption);

		Option boltParallelismOption = new Option("bp", "boltParallelism", true,
				"Parallelism for ADL Write Bolt. (Normally 4 to 6 times number of spouts). Example: 4");
		boltParallelismOption.setRequired(true);
		options.addOption(boltParallelismOption);

		Option fileRotationSizeOption = new Option("rs", "fileRotationSize", true,
				"Rotation policy size for wasb files (MB). 1024MB is a recommended size.");
		fileRotationSizeOption.setRequired(true);
		options.addOption(fileRotationSizeOption);

		Option spoutWaitTimeMsOption = new Option("swt", "spoutWaitStrategyTimeMs", true,
				"Wait time in ms for spout wait strategy.");
		spoutWaitTimeMsOption.setRequired(false);
		options.addOption(spoutWaitTimeMsOption);

		Option workerChildOptsOption = new Option("wco", "workerChildOpts", true,
				"Worker JVM options. Value is passed as is. ex -Xmx512M");
		workerChildOptsOption.setRequired(false);
		options.addOption(workerChildOptsOption);

		Option msgTimeoutOption = new Option("mt", "msgTimeoutSecs", true, "Wait time in ms for spout wait strategy.");
		msgTimeoutOption.setRequired(false);
		options.addOption(msgTimeoutOption);

		Option messageSizeOption = new Option("ms", "messageSize", true, "Size of Message in KB. Example: 128");
		messageSizeOption.setRequired(true);
		options.addOption(messageSizeOption);

		CommandLineParser parser = new GnuParser();
		HelpFormatter formatter = new HelpFormatter();
		CommandLine cmd;
		int workers = 0;
		int messageSize = 0;
		int spoutParallelism = 0;
		int boltParallelism = 0;
		int fileRotationSize = 0;
		String topologyName = null;
		String wasbUrl = null;
		String wasbFileDirPath = null;
		int spoutWaitStrategyTimeMs = 0;
		String workerChildOpts = "";
		int msgTimeoutSecs = 0;
		try {
			cmd = parser.parse(options, args);
			workers = Integer.parseInt(cmd.getOptionValue("workers"));
			messageSize = Integer.parseInt(cmd.getOptionValue("messageSize", "1024"));
			fileRotationSize = Integer.parseInt(cmd.getOptionValue("fileRotationSize"));
			spoutParallelism = Integer.parseInt(cmd.getOptionValue("spoutParallelism"));
			boltParallelism = Integer.parseInt(cmd.getOptionValue("boltParallelism"));
			topologyName = cmd.getOptionValue("topologyName");
			wasbUrl = cmd.getOptionValue("wasbRepoUrl");
			wasbFileDirPath = cmd.getOptionValue("wasbFileRoot");
			spoutWaitStrategyTimeMs = Integer.parseInt(cmd.getOptionValue("spoutWaitStrategyTimeMs", "10"));
			workerChildOpts = cmd.getOptionValue("workerChildOpts", "-Xmx4g");
			msgTimeoutSecs = Integer.parseInt(cmd.getOptionValue("msgTimeoutSecs", "300"));
		} catch (ParseException e) {
			System.out.println(e.getMessage());
			formatter.printHelp("WriteBufferTopology", options);

			System.exit(1);
			return;
		}

		int maxSpoutPending = (2 * ((fileRotationSize * 1024 * 1024)/(messageSize)));
		
		System.out.println("Using: ");
		System.out.println("Topology Name: " + topologyName);
		System.out.println("Number of workers: " + workers);
		System.out.println("Message size (bytes): " + messageSize);
		System.out.println("Spout Parallelism: " + spoutParallelism);
		System.out.println("Bolt Parallelism: " + boltParallelism);
		System.out.println("wasb File Rotation size (MB): " + fileRotationSize);
		System.out.println("wasb File Repo: " + wasbUrl);
		System.out.println("wasb File container: " + wasbFileDirPath);
		System.out.println("Max Spout Pending = " + maxSpoutPending);
		System.out.println("Worker child opts: " + workerChildOpts);
		System.out.println("MessageTimeoutSecs: " + msgTimeoutSecs);
		
		TopologyBuilder builder = new TopologyBuilder();

		TextEmitterSpout spout = new TextEmitterSpout().withPrefix("a").withSuffix("b").withFillerTextLength(messageSize-2);

		builder.setSpout("textgenerator", spout, spoutParallelism);
		FileNameFormat fileNameFormat2 = new HourlyFileNameFormat().withPath(wasbFileDirPath);
		AbstractAzureHdfsBolt wasbBolt = new AzureWasbBolt()
				.withRecordFormat(new DelimitedRecordFormat().withFieldDelimiter(","))
				.withFsUrl(wasbUrl)
				.withRotationPolicy(new FileSizeRotationPolicy(fileRotationSize, Units.MB))
				.withFileNameFormat(fileNameFormat2);

		builder.setBolt("hdfsBolt", wasbBolt, boltParallelism).localOrShuffleGrouping("textgenerator");

		Config conf = new Config();
		conf.setNumWorkers(workers);
		conf.setNumAckers(workers);
				
		conf.setMaxSpoutPending(maxSpoutPending);
		conf.put(Config.WORKER_CHILDOPTS, workerChildOpts);
		conf.put(Config.TOPOLOGY_MESSAGE_TIMEOUT_SECS, msgTimeoutSecs);
		//conf.put(Config.TOPOLOGY_SPOUT_WAIT_STRATEGY, "org.apache.storm.spout.SleepSpoutWaitStrategy");
		//conf.put(Config.TOPOLOGY_DISRUPTOR_WAIT_TIMEOUT_MILLIS, spoutWaitStrategyTimeMs);
		conf.setDebug(false);
		StormSubmitter.submitTopology(topologyName, conf, builder.createTopology());
	}
}

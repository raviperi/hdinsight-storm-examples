using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SCP;
using Microsoft.SCP.Topology;
using System.Configuration;

namespace ScpLambdaTopology
{
    [Active(true)]
    class Program : TopologyDescriptor
    {
        static void Main(string[] args)
        {
        }

        public ITopologyBuilder GetTopologyBuilder()
        {
            TopologyBuilder topologyBuilder = new TopologyBuilder("ScpLambdaTopology" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            var partitions = int.Parse(ConfigurationManager.AppSettings["EventHubPartitions"]);
            topologyBuilder.SetEventHubSpout(
            "EventHubSpout",
            new EventHubSpoutConfig(
                ConfigurationManager.AppSettings["EventHubSharedAccessKeyName"],
                ConfigurationManager.AppSettings["EventHubSharedAccessKey"],
                ConfigurationManager.AppSettings["EventHubNamespace"],
                ConfigurationManager.AppSettings["EventHubEntityPath"],
                partitions), partitions);

            List<string> javaSerializerInfo = new List<string>() { "microsoft.scp.storm.multilang.CustomizedInteropJSONSerializer" };

            /* Simple Logger Bolt */
            /*
            topologyBuilder
                .SetBolt("parserBolt", ParserBolt.Get, new Dictionary<string, List<string>>(), partitions, true)
                 .DeclareCustomizedJavaSerializer(javaSerializerInfo)
                .shuffleGrouping("EventHubSpout");
            */
            /* 
             * Use the HDInsight's implementation of WASB HDFS Bolt.
             * The bolt ensures that message boundaries are honored,
             * and that ACK'ing happens on file close operation, which 
             * is when WASB driver actually syncs and flushes the data 
             * to storage, and makes it durable.
             */
            /*
           var wasbRepoUrl = ConfigurationManager.AppSettings["wasbRepoUrl"];
           var wasbFileDirPath = ConfigurationManager.AppSettings["wasbFileDirRoot"];
           var fileRotationSize = ConfigurationManager.AppSettings["fileRotationSize"];
           var fileRotationSizeUnits = ConfigurationManager.AppSettings["fileRotationSizeUnits"];            
           JavaComponentConstructor wasbConstructor =
               JavaComponentConstructor.CreateFromClojureExpr(
                   String.Format(@"
                       (.withFileNameFormat 
                           (.withRotationPolicy 
                               (.withFsUrl 
                                   (.withRecordFormat 
                                       (com.microsoft.hdinsight.storm.hdfs.bolt.AzureWasbBolt.) 
                                       (.withFieldDelimiter 
                                           (org.apache.storm.hdfs.bolt.format.DelimitedRecordFormat.) 
                                           ""{0}""
                                       ) 
                                   )
                                   ""{1}""
                               )
                               (org.apache.storm.hdfs.bolt.rotation.FileSizeRotationPolicy. {2} {3})
                           )
                           (.withPath (com.microsoft.hdinsight.storm.hdfs.bolt.format.HourlyFileNameFormat.) ""{4}"")
                       )", String.Empty, wasbRepoUrl, fileRotationSize, fileRotationSizeUnits, wasbFileDirPath));

           topologyBuilder.SetJavaBolt("wasbBolt", wasbConstructor, 4).DeclareCustomizedJavaDeserializer(javaSerializerInfo).shuffleGrouping("EventHubSpout");
           */

            var boltConfig = new StormConfig();
            boltConfig.Set("topology.tick.tuple.freq.secs", "10");
            topologyBuilder.SetBolt("appendBlobBolt", WasbAppenderBolt.Get, new Dictionary<string, List<string>>(), 8, true)
                .DeclareCustomizedJavaSerializer(javaSerializerInfo)
                .addConfigurations(boltConfig)
                .shuffleGrouping("EventHubSpout");

            var config = new StormConfig();
            config.setMaxSpoutPending(10000);
            config.setWorkerChildOps("-Xmx4g");
            //config.Set("topology.backpressure.enable", "false");
            topologyBuilder.SetTopologyConfig(config);
            return topologyBuilder;
        }
    }
}


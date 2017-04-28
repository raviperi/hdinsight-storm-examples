using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SCP;
using Microsoft.SCP.Topology;
using System.Configuration;

namespace ScpLambdaEventGeneratorTopology
{
    [Active(true)]
    class Program : TopologyDescriptor
    {
        static void Main(string[] args)
        {
        }

        public ITopologyBuilder GetTopologyBuilder()
        {
            TopologyBuilder topologyBuilder = new TopologyBuilder("ScpLambdaEventGeneratorTopology" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            topologyBuilder.SetSpout(
                "Spout",
                Spout.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){"count"}}
                },
                1);


            var eventHubRole = ConfigurationManager.AppSettings["EventHubSharedAccessKeyName"];
            var eventHubKey = ConfigurationManager.AppSettings["EventHubSharedAccessKey"];
            var eventHubNs = ConfigurationManager.AppSettings["EventHubNamespace"];
            var eventHubPath = ConfigurationManager.AppSettings["EventHubEntityPath"];
            var partitions = int.Parse(ConfigurationManager.AppSettings["EventHubPartitions"]);

            JavaComponentConstructor boltConstructor = new JavaComponentConstructor("org.apache.storm.eventhubs.bolt.EventHubBolt",
                new List<Tuple<string, object>>()
                {
                    Tuple.Create<string, object>(JavaComponentConstructor.JAVA_LANG_STRING, eventHubRole),
                    Tuple.Create<string, object>(JavaComponentConstructor.JAVA_LANG_STRING, eventHubKey),
                    Tuple.Create<string, object>(JavaComponentConstructor.JAVA_LANG_STRING, eventHubNs),
                    Tuple.Create<string, object>(JavaComponentConstructor.JAVA_LANG_STRING, eventHubPath),
                    Tuple.Create<string, object>(JavaComponentConstructor.JAVA_PRIMITIVE_TYPE_BOOLEAN, partitions>1),
                });

            topologyBuilder.SetJavaBolt(
                "EHBolt",
                boltConstructor, partitions).shuffleGrouping("Spout");

            StormConfig conf = new StormConfig();
            conf.setDebug(false);
            conf.setNumWorkers(1);
            conf.setStatsSampleRate(0.05);
            conf.setWorkerChildOps("-Xmx2048m");
            conf.Set("topology.kryo.register", "[\"[B\"]");
            topologyBuilder.SetTopologyConfig(conf);

            return topologyBuilder;
        }
    }
}


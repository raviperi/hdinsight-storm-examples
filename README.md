# hdinsight-storm-examples

**NOTE: The examples are being rewritten to ensure compatibility with the latest HDInsight's Storm offering 
(HDI 3.6, Storm 1.1.0.x)
The layout of the repository is also being updated to organize code for easier discoverability.**

This repository contains complete and easy to use examples that demonstrate the power of Apache Storm on Microsoft Azure HDInsight.

The .Net and Java examples listed in this branch are applicable to **HDInsight v3.6 Linux Storm clusters**

For other HDInsight version specific examples please refer to the specific branch.
Supported versions:
* HDInsight v3.4
* HDInsight v3.5

## Getting Started

**Official page - [Apache Storm for Microsoft Azure HDInsight](http://azure.microsoft.com/en-us/services/hdinsight/apache-storm/)**

**Getting Started page - [Getting Started with HDInsight Storm](https://azure.microsoft.com/en-us/documentation/articles/hdinsight-storm-overview/)**

### Repository structure
1. [Examples](Examples) - Java and C# based example topologies for working with various Microsoft, and third party technologies. 
2. [lib](lib) - Java dependencies
3. [scripts](scripts) - Automation scripts
4. [Templates](templates) - Azure services templates
5. [tools](tools) - Helper tools

### Running the examples

The java based examples are built with Maven, and all examples are defined as modules.
Steps to submit an example topology:

#### Pre-Steps:
1. Execute a custom action script on HDInsight Storm cluster's Headn and Workernodes.
    
    Script Action URL: https://raw.githubusercontent.com/raviperi/script-actions/master/install-storm-hdfs-extlib/install-storm-hdfs-extlib.bash
    
    Please refer to [https://docs.microsoft.com/en-us/azure/hdinsight/hdinsight-hadoop-customize-cluster-linux](Custom Action Scripts) for a walk through of how-to run a custom action script
```
NOTE: This step will become optional in future releases of HDInsight.
The script actions copies required hadoop and hdfs libraries to Storm extlib 
folder so they are available during runtime.
```
2. For SCP.Net examples, you will need to have Azure Datalake Tools for HDInsight installed.
    - https://www.microsoft.com/en-us/download/details.aspx?id=49504

#### Building

##### Java based examples
1. Navigate to the root directory (example: c:\git\hdinsight-storm-examples)
2. Execute **mvn clean install** 
    - This will build all the maven modules and install them locally.
3. Navigate to an example project project of choice, and there will be a target directory with a shaded jar.
    example: (c:\git\hdinsight-storm-examples\examples\Simple\AzureAdlStorageWriterTopology\target
4. Upload the shaded jar to the cluster
5. Submit the jar based topology using storm cli.
    - Reference: http://storm.apache.org/releases/1.0.3/Running-topologies-on-a-production-cluster.html
    
##### SCPNet examples

1. SCPNet examples require Java libraries, so follow the steps above to build Java code first.
2. Navigate to the folder containing SCPNet projects. (Example: examples\SCPNet\EndToEnd\ScpLambdaTopology)
3. Launch the solution file
4. Update the app.config to provide values for required configuration data.
4. Use the Visual studio extensions to compile, package, and submit the topology to a Linux cluster.


## Examples
Simple (POC) and End-to-End examples of integrating  HDInsight Storm (and SCP.Net) with popular technologies.

### Examples Layout

1. [Simple](Simple) - proof-of-concept **Java** implementations to showcase HDInsight storm integration with:
    * [Windows Azure Blob Store (WASB)](examples/Simple/WasbWriterTopology)
    * [Azure Datalake Store (ADLS)](examples/Simple/AdlWriterTopology)
    * HBase
    * Elastic Search

3. [EndToEnd](EndToEnd) - Production ready **Java** implementations that showcase HDInsight Storm integration with:
    * EventHub and WASB
    * EventHub and HBase
    * Kafka and HBase
    * Kafka and ADL
    * Kafka and WASB
    * IOTHub and HBase
    * EventHub and DocumentDB
    * ServiceBus and HBase

4. [SCPNet](examples/SCPNet/EndToEnd) - Production ready implementations that showcase HDInsight Storm integration using SCPNet topologies with:
    * [Lambda Topology](examples/SCPNet/EndToEnd/ScpLambdaTopology)
        - SCP.Net based lambda topology that reads from EventHub and writes to WASB
    * EventHub and HBase
    * Kafka and HBase
    * Kafka and ADL
    * Kafka and WASB
    * IOTHub and HBase
    * EventHub and DocumentDB
    * ServiceBus and HBase

## SCP.Net
For writing Storm applications in C# using [SCP.Net](https://www.nuget.org/packages/Microsoft.SCP.Net.SDK/) refer [SCPNet-GettingStarted.md](SCPNet-GettingStarted.md)

### SCP.Net Examples
Check out more [SCP.Net Examples](SCPNetExamples)

### Topology, Spout  Bolt templates
Templates to connect to various Azure services:
* EventHubs
* SQL Azure
* DocumentDB
* HBase
* Azure Websites (SignalR)

[Templates for Azure services](templates)

## Azure Services and HDInsisght Helper PowerShell Scripts
* Looking for some Azure PowerShell scripts that you can use individually or in an automation?
  * [Azure PowerShell Scripts](scripts/azure)
* Azure PowerShell scripts to create HDInsight clusters
  * [Azure HDInsight PowerShell Scripts](scripts/azure/HDInsight)

## References
* [HDInsight] (http://azure.microsoft.com/en-us/documentation/services/hdinsight/)
* [Azure Event Hubs] (http://azure.microsoft.com/en-us/services/event-hubs/)
* [DocumentDB] (http://azure.microsoft.com/en-us/services/documentdb/)
* [SQL Azure] (http://azure.microsoft.com/en-us/services/sql-database/)

## Build status

[![Build status](https://ci.appveyor.com/api/projects/status/8s55c8pmlye9uhu8?svg=true)](https://ci.appveyor.com/project/rtandonmsft/hdinsight-storm-examples)

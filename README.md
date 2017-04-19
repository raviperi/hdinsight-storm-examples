# hdinsight-storm-examples


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
1. [Examples](Examples) - Simple and End-to-End examples of working with various Microsoft, and third party technologies.
2. [lib](lib) - Java dependencies
3. [scripts](scripts) - Automation scripts
4. [Templates](templates) - Azure services templates
5. [tools](tools) - Helper tools

### Running the examples
Each of the examples folder contains a ```run.bat``` that does the following:

1. Prepare (using ```prepare.ps1```)
  1. Creation of required services (and resources) in Azure
  2. Updating project properties and templates
2. Build (using ```build.ps1```)
  1. Building the source code and use the properties created in previous step
3. Execute (using ```execute.ps1```)
  1. Upload jars and create topology packages (as needed)
  2. Submitting the topologies to the HDInsight Storm cluster

An existing run configuration is first detected to provide idem-potency in creation of Azure resources. This allows you to re-run the ```run.bat``` in case of some unexpected failure.

### Building the examples
If you just want to build the source code you can use the ```build.bat``` in each example folder to build that example.

### Re-running the example
If you modified some source and wish to re-deploy the topologies, you can build using ```build.bat``` and submit the topologies using ```execute.ps1```

## Examples
Simple (POC) and End-to-End examples of integrating  HDInsight Storm (and SCP.Net) with popular technologies.

### Examples Layout

1. [Simple](Simple) - Proof-of-concept implementations to showcase HDInsight storm integration with:
    * [Windows Azure Blob Store (WASB)](examples/Simple/WasbWriterTopology)
    * [Azure Datalake Store (ADLS)](examples/Simple/AdlWriterTopology)
    * HBase
    * Elastic Search

2. [SCPNet - Simple](Simple/SCPNet) - proof-of-concept implementations to showcase HDInsight storm integration using SCPNet topologies with:
    * [Windows Azure Blob Store (WASB)](examples/Simple/SCPNet/WasbWriterTopology)
    * [Azure Datalake Store (ADLS)](examples/Simple/SCPNet/AdlWriterTopology)
    * HBase
    * Elastic Search
    
3. [EndToEnd](EndToEnd) - Production ready implementations that showcase HDInsight Storm integration with:
    * EventHub and HBase
    * Kafka and HBase
    * Kafka and ADL
    * Kafka and WASB
    * IOTHub and HBase
    * EventHub and DocumentDB
    * ServiceBus and HBase

4. [SCPNet - EndToEnd](EndToEnd/SCPNet) - Production ready implementations that showcase HDInsight Storm integration using SCPNet topologies with:
    * EventHub and HBase
    * Kafka and HBase
    * Kafka and ADL
    * Kafka and WASB
    * IOTHub and HBase
    * EventHub and DocumentDB
    * ServiceBus and HBase
    
**Note:** Each example comes with a paired SCPNet implementation that showcases C# based topologies to achieve the same goal as the intended example.
There are multiple end-to-end examples included in this repository that showcase different topologies that you can build and run against HDInsight Storm.
HDInsight provides users the option to write applications in language of their choice and exposes Storm REST APIs as well as additional web interfaces for remote topology submission and management.

### Internet of Things Example (Java)
An internet of things example that demonstrates connected cars scenarios. 
The example showcases the reading of vehicle events from Event Hub, referencing Azure DocumentDB for vehicular data and then finally storing the computations in Azure Storage using the WASB driver on HDInsight clusters.

Read more about this example here: [IoT example using Azure EventHubs, Storm, Azure DocumentDB, Azure Storage/WASB](IotExample)

### EventHubs/Kafka Scalability Example (Java & SCP.Net)
A scalability benchmark that demonstrates throughput from EventHub/Kafka and stores the event counts in SQL Azure.

Read more about this example here: [Event Count example using Azure EventHubs, Storm, SQL Azure](EventCountExample)

### Real Time ETL Example (SCP.Net)
A real time ETL example with EventHubs, Storm and HBase that has two topologies.
* The first topology **Event Sender Topology** generates Web Request Logs and pushes them to EventHubs.
* The second topology **Event Aggregation Topology** reads from EventHubs, calculates aggregations("count() group by") and then stores the computations in a HDInsight HBase cluster.

Read more about this example here: [Real Time ETL example using Azure EventHubs, Storm, HBase & SCP.Net](RealTimeETLExample)

## SCP.Net
For writing Storm applications in C# using [SCP.Net](https://www.nuget.org/packages/Microsoft.SCP.Net.SDK/) refer [SCPNet-GettingStarted.md](SCPNet-GettingStarted.md)

### SCP.Net Examples
Check out more [SCP.Net Examples](SCPNetExamples)

This directory now contains many SCP.Net examples that show the APIs available in SCP.Net NuGet package. Follow the instructions in the above directory to deploy these examples to your HDInsight Storm cluster.

The ```HybridTopologyHostMode``` in [SCPNetExamples/HybridTopologyHostMode](SCPNetExamples/HybridTopologyHostMode/net) has examples on all the different types of hybrid modes.

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

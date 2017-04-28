using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.SCP;
using Microsoft.SCP.Rpc.Generated;
using System.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StormLambdaCommon.hdfs.bolt;

namespace ScpLambdaTopology
{
    public class WasbAppenderBolt : ISCPBolt
    {
        private Context ctx;
        private Configuration config;
        private CloudBlobClient cloudBlobClient;
        private SyncPolicy syncPolicy;
        private RotationPolicy rotationPolicy;
        private RecordFormat recordFormat;
        private FileNameFormat fileNameFormat;
        int maxBytesPerFile;
        //long syncedByteCount;
        long fileByteCount;
        List<SCPTuple> tupleBuffer = new List<SCPTuple>();
        private CloudBlobContainer currentContainer;
        private CloudAppendBlob currentAppendBlob;
        private bool enableAck;
        private int rotation = 1;
        public WasbAppenderBolt(Context ctx, Dictionary<string, Object> parms)
        {
            this.ctx = ctx;
            if (parms != null && parms.ContainsKey(Constants.USER_CONFIG))
            {
                this.config = (Configuration)parms[Constants.USER_CONFIG];
            }

            if (config == null)
            {
                throw new Exception("Failed to read user configuration. No configuration object found in property bag.");
            }

            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add("default", new List<Type>() { typeof(string) });
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
            this.ctx.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());
            if (Context.Config.pluginConf.ContainsKey(Microsoft.SCP.Constants.NONTRANSACTIONAL_ENABLE_ACK))
            {
                enableAck = (bool)(Context.Config.pluginConf[Microsoft.SCP.Constants.NONTRANSACTIONAL_ENABLE_ACK]);
            }
            Context.Logger.Info("Acking is enabled: " + enableAck);

            Prepare();
        }

        public static WasbAppenderBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new WasbAppenderBolt(ctx, parms);
        }

        private string GetFileName()
        {
            var blobName = fileNameFormat.GetPath() + fileNameFormat.GetName(rotation, DateTime.UtcNow.Ticks);
            return blobName;
        }

        private void Prepare()
        {
            Context.Logger.Info("Preparing Appender Bolt");
            int rotationSize = int.Parse(config.AppSettings.Settings["BlobStorageFileRotationSize"].Value);
            FileSizeUnit rotationSizeUnits = (FileSizeUnit)Enum.Parse(typeof(FileSizeUnit), config.AppSettings.Settings["BlobStorageFileRotationSizeUnits"].Value);
            rotationPolicy = new FileSizeRotationPolicy(rotationSize, rotationSizeUnits);

            string fileNamePrefix = config.AppSettings.Settings["BlobStorageFilenamePrefix"].Value;
            string rootPath = config.AppSettings.Settings["BlobStorageRootPath"].Value;
            fileNameFormat = new HourlyFileNameFormat().WithPrefix(fileNamePrefix).WithPath(rootPath).WithExtension(".txt");

            //int syncSize = int.Parse(config.AppSettings.Settings["BlobStorageSyncSize"].Value);
            int syncCount = int.Parse(config.AppSettings.Settings["BlobStorageSyncCount"].Value);
            //syncPolicy = new SizeSyncPolicy(syncSize);
            syncPolicy = new CountSyncPolicy(syncCount);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config.AppSettings.Settings["BlobStorageConnectionString"].Value);
            string storageContainer = config.AppSettings.Settings["BlobStorageContainer"].Value;

            cloudBlobClient = storageAccount.CreateCloudBlobClient();
            currentContainer = cloudBlobClient.GetContainerReference(storageContainer);
            currentContainer.CreateIfNotExists();

            var blobName = GetFileName();
            Context.Logger.Info("Blob path: " + blobName);
            currentAppendBlob = currentContainer.GetAppendBlobReference(blobName);
            currentAppendBlob.CreateOrReplace();
            Context.Logger.Info("Prepared Appender Bolt");
        }

        private void RotateBlob()
        {
            ++rotation;
            var newPath = GetFileName();
            Context.Logger.Info("Blob path: " + newPath);
            currentAppendBlob = currentContainer.GetAppendBlobReference(newPath);
            currentAppendBlob.CreateOrReplace();
            fileByteCount = 0;
        }

        private void Sync()
        {
            currentAppendBlob.AppendText(String.Join("\n", tupleBuffer.Select(t => t.GetString(0))));
            //syncedByteCount = 0;

            if (enableAck)
            {
                Context.Logger.Info("Acking tuples");
                foreach (var tuple in tupleBuffer)
                {
                    ctx.Ack(tuple);
                }
            }
            tupleBuffer.Clear();
        }

        public void Execute(SCPTuple tuple)
        {            
            try
            {
                //Context.Logger.Info("Processing tuple in appender bolt.");
                string message = null;
                var forceSync = false;
                var len = 0;
                if (tuple.GetSourceStreamId() == Constants.SYSTEM_TICK_STREAM_ID)
                {
                    Context.Logger.Info("Tick tuple");
                    forceSync = true;
                }
                else
                {
                    tupleBuffer.Add(tuple);
                    message = tuple.GetString(0);                    
                    len = Encoding.UTF8.GetByteCount(message);
                    fileByteCount += len;
                }

                if (forceSync || syncPolicy.Mark(tuple, len))
                {
                    Context.Logger.Info("Syncing tuples.");
                    Sync();
                }

                if (rotationPolicy.Mark(tuple, fileByteCount))
                {
                    Context.Logger.Info("Rotating blob " + fileByteCount);
                    RotateBlob();
                }                
                //syncedByteCount += len;
                
        }
            catch (Exception e)
            {
                Context.Logger.Error(e);
                throw;
            }
        }
    }
}
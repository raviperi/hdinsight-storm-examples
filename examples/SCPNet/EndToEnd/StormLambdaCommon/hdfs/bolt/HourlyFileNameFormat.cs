using Microsoft.SCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLambdaCommon.hdfs.bolt
{
    public sealed class HourlyFileNameFormat : FileNameFormat
    {
        private string prefix;
        private string extension;
        private string path;
        private Context ctx;
        
        public HourlyFileNameFormat WithPrefix(string prefix)
        {
            this.prefix = prefix;
            return this;
        }

        public HourlyFileNameFormat WithPath(String path)
        {
            this.path = path;
            return this;
        }

        public HourlyFileNameFormat WithExtension(string extension)
        {
            this.extension = extension;
            return this;
        }

        public string GetName(long rotation, long timestamp)
        {
            var taskId = Context.TopologyContext.GetThisTaskId();
            var component = Context.TopologyContext.GetComponentId(taskId);
            return String.Format("{0}-{1}-{2}-{3}-{4}{5}",
                this.prefix,
                component,
                taskId,
                rotation,
                timestamp,
                extension);            
        }

        public string GetPath()
        {
            return String.Format("{0}{1}", this.path, DateTime.UtcNow.ToString("yyyy/MM/dd/HH/"));
        }
    }
}

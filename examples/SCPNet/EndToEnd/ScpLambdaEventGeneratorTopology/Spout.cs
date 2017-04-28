using System;
using System.Collections.Generic;
using Microsoft.SCP;
using StormLambdaCommon;
using System.Configuration;

namespace ScpLambdaEventGeneratorTopology
{
    public class Spout : ISCPSpout
    {
        private Context ctx;
        private Configuration config;
        private Random r = new Random();
        private List<String> deviceIds = new List<String>();
        private int deviceIdMin;
        private int deviceIdMax;
        private int eventCount = 0;
        private int maxEventCount = 1000;

        public Spout(Context ctx, Dictionary<string, object> parms = null)
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

            deviceIdMin = int.Parse(config.AppSettings.Settings["DeviceIdStart"].Value);
            deviceIdMax = deviceIdMin + int.Parse(config.AppSettings.Settings["DeviceCount"].Value);
            maxEventCount = int.Parse(config.AppSettings.Settings["EventCount"].Value);

            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add("default", new List<Type>() { typeof(string) });
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(null, outputSchema));
        }

        public static Spout Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new Spout(ctx, parms);
        }

        public void NextTuple(Dictionary<string, Object> parms)
        {
            int devId = deviceIdMin;
            while (eventCount <= maxEventCount)
            {
                var d = new DeviceEvent
                {
                    DeviceCategory = "Category1",
                    DeviceId = devId,
                    DeviceVersion = "1.0",
                    Temparature = r.NextDouble(),
                    TimeStamp = DateTime.Now
                };
                ++devId;
                if (devId > deviceIdMax)
                {
                    devId = deviceIdMin;
                }

                ctx.Emit(new Values(d.ToXml()));
                ++eventCount;
            }
        }

        public void Ack(long seqId, Dictionary<string, Object> parms)
        {
            //ignore
        }

        public void Fail(long seqId, Dictionary<string, Object> parms)
        {
            //ignore
        }
    }
}
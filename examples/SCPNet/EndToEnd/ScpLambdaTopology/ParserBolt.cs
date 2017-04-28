using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.SCP;
using Microsoft.SCP.Rpc.Generated;

namespace ScpLambdaTopology
{
    public class ParserBolt : ISCPBolt
    {
        private Context ctx;

        public ParserBolt(Context ctx)
        {
            this.ctx = ctx;

            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add("default", new List<Type>() { typeof(string) });
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public static ParserBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new ParserBolt(ctx);
        }

        public void Execute(SCPTuple tuple)
        {
            try
            {                
                Context.Logger.Info("Tuple: " + tuple.GetValue(0));
                ctx.Ack(tuple);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                ctx.Fail(tuple);
            }
        }
    }
}
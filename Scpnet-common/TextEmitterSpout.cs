using Microsoft.SCP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scpnet_common
{
    public class TextEmitterSpout : ISCPSpout
    {
        private Context ctx;
        private Configuration cfg;
        private Dictionary<long, String> cache = new Dictionary<long, string>();
        private string prefix = String.Empty;
        private string suffix = String.Empty;
        private string fixedString = String.Empty;
        private long sequenceId;
        private bool isReliable;

        public TextEmitterSpout(Context ctx, Dictionary<string, Object> parms = null)
        {
            this.ctx = ctx;
            // Demo how to get User customized config from parms 
            if (parms != null && parms.ContainsKey(Constants.USER_CONFIG))
            {
                this.cfg = (Configuration)parms[Constants.USER_CONFIG];
            }

            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add("default", new List<Type>() { typeof(string) });
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(null, outputSchema));
        }

        public TextEmitterSpout WithPrefix(string prefix)
        {
            this.prefix = prefix;
            return this;
        }

        public TextEmitterSpout WithSuffix(string suffix)
        {
            this.suffix = suffix;
            return this;
        }

        public TextEmitterSpout WithFillerStringLength(int len)
        {
            fixedString = new String('$', len);
            return this;
        }

        public TextEmitterSpout WithReliability(bool isReliable)
        {
            this.isReliable = isReliable;
            return this;
        }


        /// <summary>
        /// This method is used to emit one or more tuples. If there is nothing to emit, this method should return without emitting anything. 
        /// It should be noted that NextTuple(), Ack(), and Fail() are all called in a tight loop in a single thread in C# process. 
        /// When there are no tuples to emit, it is courteous to have NextTuple sleep for a short amount of time (such as 10 milliseconds), so as not to waste too much CPU.
        /// </summary>
        /// <param name="parms"></param>
        public void NextTuple(Dictionary<string, Object> parms)
        {
            Context.Logger.Info("NextTuple enter");
            string sentence;

            sequenceId++;        
            this.ctx.Emit(Constants.DEFAULT_STREAM_ID, new Values(sentence), lastSeqId);
                    cachedTuples[lastSeqId] = sentence;
                }
                else
                {
                    // if have nothing to emit, then sleep for a little while to release CPU
                    Thread.Sleep(50);
                }
                Context.Logger.Info("cached tuple num: {0}", cachedTuples.Count);
            }
            else
            {
                sentence = sentences[rand.Next(0, sentences.Length - 1)];
                Context.Logger.Info("Emit: {0}", sentence);
                this.ctx.Emit(new Values(sentence));
            }

            Context.Logger.Info("NextTx exit");
        }

        /// <summary>
        /// Ack() will be called only when ack mechanism is enabled in spec file.
        /// If ack is not supported in non-transactional topology, the Ack() can be left as empty function. 
        /// </summary>
        /// <param name="seqId">Sequence Id of the tuple which is acked.</param>
        /// <param name="parms"></param>
        public void Ack(long seqId, Dictionary<string, Object> parms)
        {
            Context.Logger.Info("Ack, seqId: {0}", seqId);
            bool result = cachedTuples.Remove(seqId);
            if (!result)
            {
                Context.Logger.Warn("Ack(), remove cached tuple for seqId {0} fail!", seqId);
            }
        }

        /// <summary>
        /// Fail() will be called only when ack mechanism is enabled in spec file. 
        /// If ack is not supported in non-transactional topology, the Fail() can be left as empty function.
        /// </summary>
        /// <param name="seqId">Sequence Id of the tuple which is failed.</param>
        /// <param name="parms"></param>
        public void Fail(long seqId, Dictionary<string, Object> parms)
        {
            Context.Logger.Info("Fail, seqId: {0}", seqId);
            if (cachedTuples.ContainsKey(seqId))
            {
                string sentence = cachedTuples[seqId];
                Context.Logger.Info("Re-Emit: {0}, seqId: {1}", sentence, seqId);
                this.ctx.Emit(Constants.DEFAULT_STREAM_ID, new Values(sentence), seqId);
            }
            else
            {
                Context.Logger.Warn("Fail(), can't find cached tuple for seqId {0}!", seqId);
            }
        }

        /// <summary>
        ///  Implements of delegate "newSCPPlugin", which is used to create a instance of this spout/bolt
        /// </summary>
        /// <param name="ctx">SCP Context instance</param>
        /// <param name="parms">Parameters to initialize this spout/bolt</param>
        /// <returns></returns>
        public static Generator Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new Generator(ctx, parms);
        }
    }
}

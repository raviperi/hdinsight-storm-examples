
namespace StormLambdaCommon.hdfs.bolt
{
    using Microsoft.SCP;

    /// <summary>
    /// Reference implements that mimics the behavior of Storm-hdfs CountSyncPolicy
    /// </summary>
    public sealed class CountSyncPolicy: SyncPolicy
    {
        private int syncCount;
        private int executedCount;

        public CountSyncPolicy(int syncCount)
        {
            this.syncCount = syncCount;
        }
        
        public bool Mark(SCPTuple tuple, long offset)
        {
            this.executedCount++;
            return this.executedCount > this.syncCount;
        }

        public void reset()
        {
            this.executedCount = 0;
        }
    }
}

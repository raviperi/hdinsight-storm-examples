namespace StormLambdaCommon.hdfs.bolt
{
    using Microsoft.SCP;

    /// <summary>
    /// Reference implementation that is customized to 
    /// the way WASB or ADLS based hdfs drivers work,
    /// where there is a 4MB client buffer that is used to 
    /// hold and push bits to the server.
    /// 
    /// To honor message boundaries, the size sync policy
    /// ensures that a tuple does not get split across 
    /// flush sessions.
    /// </summary>
    public sealed class SizeSyncPolicy : SyncPolicy
    {
        private long bufferMaxCapacity;

        public SizeSyncPolicy(long maxCapacity)
        {
            this.bufferMaxCapacity = maxCapacity;
        }

        public bool Mark(SCPTuple tuple, long offset)
        {
            if ((offset) > bufferMaxCapacity)
            {
                return true;
            }

            return false;
        }

        public void reset()
        {
        }
    }
}


using System;
using Microsoft.SCP;

namespace StormLambdaCommon.hdfs.bolt
{
    /// <summary>
    /// Rotation policy that is based on number of 
    /// bytes written to a file.
    /// </summary>
    public sealed class FileSizeRotationPolicy : RotationPolicy
    {
        private long byteCount;
        private readonly long maxBytes;

        public FileSizeRotationPolicy(float count, FileSizeUnit units)
        {
            switch (units)
            {
                case FileSizeUnit.KB:
                    maxBytes = (long)Math.Pow(2, 10);
                    break;
                case FileSizeUnit.MB:
                    maxBytes = (long)Math.Pow(2, 20);
                    break;
                case FileSizeUnit.GB:
                    maxBytes = (long)Math.Pow(2, 30);
                    break;
                case FileSizeUnit.TB:
                    maxBytes = (long)Math.Pow(2, 40);
                    break;
                default:
                    throw new Exception("Invalid file size unit specified: " + units);
            }
        }

        public bool Mark(SCPTuple tuple, long offset)
        {
            return (offset >= this.maxBytes);
        }

        public void reset()
        {
            throw new NotImplementedException();
        }
    }
}

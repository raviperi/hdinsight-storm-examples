
using Microsoft.SCP;
using System.Collections.Generic;
using System.Text;

namespace StormLambdaCommon.hdfs.bolt
{
    /// <summary>
    /// Outputs a tuple as a delimited string.
    /// Default field delimiter is "," and default row delimiter is "\n".
    /// </summary>
    public sealed class DelimitedRecordFormat : RecordFormat
    {
        public const string DefaultFieldDelimiter = ",";
        public const string DefaultRecordDelimiter = "\n";
        private string fieldDelimiter = DefaultFieldDelimiter;
        private string recordDelimiter = DefaultRecordDelimiter;
        private Fields fields = null;

        public DelimitedRecordFormat WithFieldDelimiter(string delimiter)
        {
            this.fieldDelimiter = delimiter;
            return this;
        }

        public DelimitedRecordFormat WithRecordDelimiter(string delimiter)
        {
            this.recordDelimiter = delimiter;
            return this;
        }

        public byte[] format(SCPTuple tuple)
        {
            List<object> retList = new List<object>();
            for (int i = 0; i < tuple.Size(); ++i)
            {
                retList.Add(tuple.GetValue(i));
            }

            return Encoding.UTF8.GetBytes(string.Format("{0}{1}", string.Join(this.fieldDelimiter, retList), this.recordDelimiter));
        }
    }
}

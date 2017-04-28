using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLambdaCommon.hdfs.bolt
{
    public interface FileNameFormat
    {
        string GetName(long rotation, long timestamp);

        string GetPath();
    }
}

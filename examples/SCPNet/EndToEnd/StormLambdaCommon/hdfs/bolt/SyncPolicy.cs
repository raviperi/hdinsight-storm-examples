using Microsoft.SCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLambdaCommon.hdfs.bolt
{
    public interface SyncPolicy
    {
        bool Mark(SCPTuple tuple, long offset);

        void reset();
    }
}

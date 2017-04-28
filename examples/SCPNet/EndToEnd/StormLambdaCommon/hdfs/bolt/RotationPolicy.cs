namespace StormLambdaCommon.hdfs.bolt
{
    using Microsoft.SCP;

    /// <summary>
    /// Used by HDFS bolt in determining if a HDFS file should be rotated.
    /// For every tuple, before it is processed a call is made to check if
    /// rotation is needed.
    /// </summary>
    public interface RotationPolicy
    {
        /// <summary>
        /// Called for every tuple.
        /// The offset passed is a cumulative count of number
        /// of bytes written so far.
        /// </summary>
        /// <param name="tuple">SCP Tuple</param>
        /// <param name="offset">Total number of bytes written so far</param>
        /// <returns>true if file is to be rotated, false otherwise</returns>
        bool Mark(SCPTuple tuple, long offset);

        /// <summary>
        /// Called after HDFS bolt rotates a file
        /// </summary>
        void reset();
    }
}

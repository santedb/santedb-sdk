using MohawkCollege.Util.Console.Parameters;
using System;
using System.ComponentModel;

namespace BackupConverter
{
    /// <summary>
    /// Console parameters
    /// </summary>
    public class ConsoleParameters
    {

        /// <summary>
        /// The source files
        /// </summary>
        [Parameter("source")]
        [Parameter("s")]
        [Description("The source sdbk file")]
        public String Source { get; set; }

        /// <summary>
        /// The destination
        /// </summary>
        [Description("The output directory")]
        [Parameter("out")]
        [Parameter("o")]
        public String Destination { get; set; }

        /// <summary>
        /// SHow help
        /// </summary>
        [Parameter("Show help and exit")]
        [Parameter("help")]
        public bool Help { get; set; }
    }
}

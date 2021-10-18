using MohawkCollege.Util.Console.Parameters;
using System.ComponentModel;

namespace PakSrv
{
    /// <summary>
    /// PakMan package manager parameters
    /// </summary>
    public class PakSrvParameters
    {

        /// <summary>
        /// Console mode
        /// </summary>
        [Parameter("console")]
        [Description("Run the application in console mode")]
        public bool Console { get; set; }

        /// <summary>
        /// Install the service
        /// </summary>
        [Parameter("install")]
        [Description("Install the service into Windows service manager")]
        public bool Install { get; set; }

        /// <summary>
        /// Uninstall the service 
        /// </summary>
        [Parameter("uninstall")]
        [Description("Remove the service from the Windows service manager")]
        public bool Uninstall { get; set; }
    }
}

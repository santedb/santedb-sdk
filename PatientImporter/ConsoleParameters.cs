using MohawkCollege.Util.Console.Parameters;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PatientImporter
{
    /// <summary>
    /// Represents console parameters for the fake data generator
    /// </summary>
    public class ConsoleParameters
    {
        /// <summary>
        /// Gets or sets concurrency
        /// </summary>
        [Parameter("concurrency")]
        [Description("Identifies the concurrency to use when sending patients")]
        public String Concurrency { get; set; }

        /// <summary>
        /// Gets or sets the realm to which the service should connect
        /// </summary>
        [Parameter("realm")]
        [Description("The realm to which data should be sent")]
        public String Realm { get; set; }

        /// <summary>
        /// Username to authenticate with
        /// </summary>
        [Parameter("user")]
        [Description("The user to use when authenticating")]
        public String UserName { get; set; }

        /// <summary>
        /// Password to authenticate with
        /// </summary>
        [Parameter("password")]
        [Description("The password of the user to authenticate with")]
        public String Password { get; set; }

        /// <summary>
        /// Show help
        /// </summary>
        [Parameter("help")]
        [Description("Show help and exit")]
        public bool Help { get; set; }

        /// <summary>
        /// Source of files
        /// </summary>
        [Parameter("source")]
        [Parameter("*")]
        [Description("Source files to process")]
        public StringCollection Source { get; set; }

        /// <summary>
        /// Gets or sets teh 
        /// </summary>
        [Parameter("eid")]
        [Description("Authority of EnterpriseID")]
        public String EnterpriseIdDomain { get; set; }

        /// <summary>
        /// Gets or sets the MRN authority
        /// </summary>
        [Parameter("mrn")]
        [Description("Authority of MRN")]
        public String MrnDomain { get; set; }

        /// <summary>
        /// Gets the domain for the SSN
        /// </summary>
        [Parameter("ssn")]
        [Description("Authority of SSN")]
        public String SsnDomain { get; set; }

    }
}

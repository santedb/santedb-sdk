using MohawkCollege.Util.Console.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataGenerator
{
    /// <summary>
    /// Represents console parameters for the fake data generator
    /// </summary>
    public class ConsoleParameters
    {
        /// <summary>
        /// Barcode auth
        /// </summary>
        [Parameter("auth")]
        [Description("The assigning authority from which a random ID should be generated")]
        public String IdentityDomain { get; set; }

        /// <summary>
        /// Gets or sets the population size
        /// </summary>
        [Parameter("maxage")]
        [Description("The maximum age of patients to generate")]
        public String MaxAge { get; set; }
       
        /// <summary>
        /// Gets or sets the population size
        /// </summary>
        [Parameter("popsize")]
        [Description("Population size of the generated dataset")]
        public String PopulationSize { get; set; }

        /// <summary>
        /// Gets or sets concurrency
        /// </summary>
        [Parameter("concurrency")]
        [Description("Identifies the concurrency to use when seeding patients")]
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

    }
}

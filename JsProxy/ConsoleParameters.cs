using MohawkCollege.Util.Console.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsProxy
{
    /// <summary>
    /// Console parameters to the JS proxy
    /// </summary>
    public class ConsoleParameters
    {

        [Parameter("asm")]
        [Description("The compiled OpenIZ binary from which to operate")]
        public String AssemblyFile { get; set; }

        [Parameter("xml")]
        [Description("The .NET XML documentation file related to the assembly passed by --asm")]
        public String DocumentationFile { get; set; }

        [Parameter("out")]
        [Description("The output file which should be generated")]
        public String Output { get; set; }

        [Parameter("noabs")]
        [Description("When specified indicates no abstract types should be emitted")]
        public bool NoAbstract { get; internal set; }
    }

}

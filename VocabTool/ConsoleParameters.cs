using MohawkCollege.Util.Console.Parameters;
using System.ComponentModel;

namespace VocabTool
{
    /// <summary>
    /// The parameters to be passed to the program via the command line
    /// </summary>
    public class ConsoleParameters
    {

        /// <summary>
        /// Gets or sets the source file to be processed
        /// </summary>
        [Parameter("source")]
        [Parameter("s")]
        [Description("The source excel file to process")]
        public string SourceFile { get; set; }

        /// <summary>
        /// Create the specified concept.
        /// </summary>
        [Parameter("concepts")]
        [Description("Create the necessary instructions to create the concept")]
        public bool CreateConcept { get; set; }

        /// <summary>
        /// Gets or sets the name of the dataset being created
        /// </summary>
        [Parameter("name")]
        [Description("The name of the emitted dataset")]
        public string Name { get; set; }

        /// <summary>
        /// Prefix of the mnemonic
        /// </summary>
        [Parameter("prefix")]
        [Description("The prefix to add to each mnemonic")]
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the output file
        /// </summary>
        [Parameter("output")]
        [Parameter("o")]
        [Description("The output dataset file to emit")]
        public string OutputFile { get; set; }

    }
}

/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justin
 * Date: 2018-6-27
 */
using MohawkCollege.Util.Console.Parameters;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PakMan
{
    /// <summary>
    /// Console parameters
    /// </summary>
    public class PakManParameters
    {

     
        /// <summary>
        /// Clean
        /// </summary>
        [Parameter("c")]
        [Description("Instructs the compiler to clean the output directory")]
        public bool Clean { get; set; }

        /// <summary>
        /// Source files
        /// </summary>
        [Parameter("s")]
        [Parameter("source")]
        [Description("Identifies the source files to include in the applet")]
        public String Source { get; set; }

        /// <summary>
        /// Gets or sets the output
        /// </summary>
        [Description("The output applet file")]
        [Parameter("o")]
        [Parameter("output")]
        public String Output { get; set; }

        /// <summary>
        /// Gets or sets the indicator for showing help
        /// </summary>
        [Parameter("?")]
        [Parameter("help")]
        [Description("Shows this help and exits")]
        public bool Help { get; set; }

        /// <summary>
        /// Optimize the output files
        /// </summary>
        [Parameter("optimize")]
        [Description("When true, optimize (minify) javascript and css")]
        public bool Optimize { get; set; }

        /// <summary>
        /// The key that should be used to sign the applet
        /// </summary>
        [Parameter("keyFile")]
        [Description("The RSA key used to sign the applet")]
        public String SignKey { get; set; }

        /// <summary>
        /// The key used to sign the applet
        /// </summary>
        [Parameter("keyPassword")]
        [Description("The password for the applet signing key")]
        public String SignPassword { get; set; }

        /// <summary>
        /// Compile instruction
        /// </summary>
        [Parameter("compile")]
        [Description("Initiates a compilation of the specified applet source directories")]
        public bool Compile { get; internal set; }

        /// <summary>
        /// Signing instruction
        /// </summary>
        [Parameter("sign")]
        [Description("Signs an already existing applet pak file")]
        public bool Sign { get; internal set; }

        /// <summary>
        /// Embed certificate into the manifest
        /// </summary>
        [Parameter("embedCert")]
        [Description("Embeds the certificate used to sign the package in the applet (recommended for wide-publishing)")]
        public bool EmbedCertificate { get; set; }

        /// <summary>
        /// Compose
        /// </summary>
        [Parameter("compose")]
        [Description("Indicates the source files are PAK files that should be composed into a solution")]
        public bool Compose { get; set; }

        /// <summary>
        /// Install the package
        /// </summary>
        [Parameter("install")]
        [Description("Install the output package into the local repository")]
        public bool Install { get; set; }
    }
}

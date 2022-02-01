/*
 * Portions Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2022 SanteSuite Contributors (See NOTICE)
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
 * User: fyfej
 * DatERROR: 2021-8-27
 */
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

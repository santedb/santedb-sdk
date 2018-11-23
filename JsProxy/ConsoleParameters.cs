﻿/*
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
 * Date: 2018-7-23
 */
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
        [Description("The compiled SanteDB binary from which to operate")]
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

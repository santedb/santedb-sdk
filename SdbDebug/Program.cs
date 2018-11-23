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
 * Date: 2018-6-27
 */
using MohawkCollege.Util.Console.Parameters;
using SdbDebug.Options;
using SdbDebug.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SdbDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SanteDB Business Rule & CDSS Debugger v{0} ({1})", Assembly.GetEntryAssembly().GetName().Version, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Console.WriteLine("Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology");

            ParameterParser<DebuggerParameters> parser = new ParameterParser<DebuggerParameters>();
            var parameters = parser.Parse(args);

            if (parameters.Help)
                parser.WriteHelp(Console.Out);
            else if (parameters.Protocol)
                new ProtoDebugger(parameters).Debug();
            else if (parameters.BusinessRule)
                new BreDebugger(parameters).Debug();
            else
                Console.WriteLine("Nothing to do!");
        }
    }
}

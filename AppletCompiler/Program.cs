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
using SanteDB.Core.Applets;
using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PakMan
{
    class Program
    {

        /// <summary>
        /// The main program
        /// </summary>
        static int Main(string[] args)
        {

            Console.WriteLine("SanteDB HTML Applet Compiler v{0} ({1})", Assembly.GetEntryAssembly().GetName().Version, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Console.WriteLine("Copyright (C) 2015-2019 See NOTICE for contributors");

            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Console.WriteLine("Could not complete operation - {0}", e.ExceptionObject);
            };

            ParameterParser<PakManParameters> parser = new ParameterParser<PakManParameters>();
            var parameters = parser.Parse(args);

            if (parameters.Help)
            {
                parser.WriteHelp(Console.Out);
                return 0;
            }
            else if (parameters.Compose)
                return new Composer(parameters).Compose();
            else if (parameters.Compile)
                return new Packer(parameters).Compile();
            else if (parameters.Sign)
                return new Signer(parameters).Sign();
            else
            {
                Console.WriteLine("Nothing to do!");
                return 0;
            }
        }
      
    }
}

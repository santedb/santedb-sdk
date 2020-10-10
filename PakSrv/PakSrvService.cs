/*
 * Portions Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PakSrv
{
    public partial class PakSrvService : ServiceBase
    {

        /// <summary>
        /// SanteDB Service
        /// </summary>
        public PakSrvService()
        {
            InitializeComponent();
            this.ServiceName = "SanteDB PakSrv";
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
            
                EventLog.WriteEntry("SanteDB Package Host Service", $"Service is ready to accept connections", EventLogEntryType.Information);

            }
            catch (Exception e)
            {
                Trace.TraceError("The service reported an error: {0}", e);
                EventLog.WriteEntry("SanteDB Package Host Service", $"Service Startup reported an error: {e}", EventLogEntryType.Error, 1911);
                Environment.FailFast($"Error starting service: {e.Message}");
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry("SanteDB Package Host Service", $"Gateway has been shutdown successfully", EventLogEntryType.Information);

            }
            catch (Exception e)
            {
                Trace.TraceError("The service reported an error on shutdown: {0}", e);
                EventLog.WriteEntry("SanteDB Package Host Service", $"Service Shutdown reported an error: {e}", EventLogEntryType.Error, 1911);

                Environment.FailFast($"Error stopping service: {e.Message}");

            }
        }
    }
}

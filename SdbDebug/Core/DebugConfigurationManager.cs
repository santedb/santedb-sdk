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
using SanteDB.Cdss.Xml;
using SanteDB.Core.Configuration;
using SanteDB.Core.Data;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Protocol;
using SanteDB.Core.Services.Impl;
using SanteDB.DisconnectedClient.Core;
using SanteDB.DisconnectedClient.Core.Caching;
using SanteDB.DisconnectedClient.Core.Configuration;
using SanteDB.DisconnectedClient.Core.Data;
using SanteDB.DisconnectedClient.Core.Security;
using SanteDB.DisconnectedClient.Core.Services.Local;
using SanteDB.DisconnectedClient.SQLite;
using SanteDB.DisconnectedClient.SQLite.Connection;
using SanteDB.DisconnectedClient.SQLite.Security;
using SanteDB.DisconnectedClient.Xamarin.Configuration;
using SanteDB.DisconnectedClient.Xamarin.Diagnostics;
using SanteDB.DisconnectedClient.Xamarin.Http;
using SanteDB.DisconnectedClient.Xamarin.Net;
using SanteDB.DisconnectedClient.Xamarin.Rules;
using SanteDB.DisconnectedClient.Xamarin.Services;
using SanteDB.DisconnectedClient.Xamarin.Threading;
using SdbDebug.Options;
using SdbDebug.Shell;
using System;
using System.Collections.Generic;
using System.IO;

namespace SdbDebug.Core
{
    /// <summary>
    /// Represents a configuration manager which is used for the debugger
    /// </summary>
    public class DebugConfigurationManager : IConfigurationPersister
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DebugConfigurationManager));

        
        // Configuration path
        private readonly string m_configPath = String.Empty;

        // Data path
        private readonly string m_dataPath = string.Empty;

        /// <summary>
        /// Creates a new debug configuration manager with the specified parameters
        /// </summary>
        public DebugConfigurationManager(DebuggerParameters parms)
        {
            // Get parameters from args
            if (!String.IsNullOrEmpty(parms.ConfigurationFile))
                this.m_configPath = parms.ConfigurationFile;
            if (!String.IsNullOrEmpty(parms.DatabaseFile))
                this.m_dataPath = parms.DatabaseFile;

        }

        /// <summary>
        /// Gets the application data directory
        /// </summary>
        public string ApplicationDataDirectory
        {
            get
            {
                if (this.m_dataPath != null)
                    return Path.GetDirectoryName(this.m_dataPath);
                else
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SanteDBDC");
            }
        }

        /// <summary>
        /// Returns true if the 
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Perform a backup
        /// </summary>
        public void Backup(SanteDBConfiguration config)
        {
            throw new NotImplementedException("Debug environment cannot backup");
        }

        /// <summary>
        /// Get whether there is a backup
        /// </summary>
        /// <returns></returns>
        public bool HasBackup()
        {
            return false;
        }

        /// <summary>
        /// Load the configuration file
        /// </summary>
        public SanteDBConfiguration Load()
        {
            if (!String.IsNullOrEmpty(this.m_configPath))
                using (var fs = File.OpenRead(this.m_configPath))
                {
                    return SanteDBConfiguration.Load(fs);
                }
            else
            {
                var retVal = new SanteDBConfiguration();

                // Inital data source
                DataConfigurationSection dataSection = new DataConfigurationSection()
                {
                    MainDataSourceConnectionStringName = "santeDbData",
                    MessageQueueConnectionStringName = "santeDbData",
                    ConnectionString = new System.Collections.Generic.List<ConnectionString>() {
                    new ConnectionString () {
                        Name = "santeDbData",
                        Value = String.IsNullOrEmpty(this.m_dataPath) ?
                            Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "Minims","SanteDB.sqlite") :
                            this.m_dataPath
                    }
                }
                };

                // Initial Applet configuration
                AppletConfigurationSection appletSection = new AppletConfigurationSection()
                {
                    Security = new AppletSecurityConfiguration()
                    {
                        AllowUnsignedApplets = true,
                        TrustedPublishers = new List<string>() { "84BD51F0584A1F708D604CF0B8074A68D3BEB973" }
                    }
                };

                // Initial applet style
                ApplicationConfigurationSection appSection = new ApplicationConfigurationSection()
                {
                    Style = StyleSchemeType.Dark,
                    UserPrefDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SdbDebug", "userpref"),
                    ServiceTypes = new List<string>() {
                    typeof(DefaultPolicyDecisionService).AssemblyQualifiedName,
                    typeof(SQLitePolicyInformationService).AssemblyQualifiedName,
                    typeof(LocalRepositoryService).AssemblyQualifiedName,
                    //typeof(LocalAlertService).AssemblyQualifiedName,
                    typeof(LocalTagPersistenceService).AssemblyQualifiedName,
                    typeof(NetworkInformationService).AssemblyQualifiedName,
                    typeof(BusinessRulesDaemonService).AssemblyQualifiedName,
                    typeof(PersistenceEntitySource).AssemblyQualifiedName,
                    typeof(MemoryCacheService).AssemblyQualifiedName,
                    typeof(SanteDBThreadPool).AssemblyQualifiedName,
                    typeof(MemorySessionManagerService).AssemblyQualifiedName,
                    typeof(AmiUpdateManager).AssemblyQualifiedName,
                    typeof(AppletClinicalProtocolRepository).AssemblyQualifiedName,
                    typeof(MemoryQueryPersistenceService).AssemblyQualifiedName,
                    typeof(SimpleQueueFileProvider).AssemblyQualifiedName,
                    typeof(SimpleCarePlanService).AssemblyQualifiedName,
                    typeof(SimplePatchService).AssemblyQualifiedName,
                    typeof(DebugAppletManagerService).AssemblyQualifiedName,
                    typeof(SQLiteConnectionManager).AssemblyQualifiedName,
                    typeof(SQLitePersistenceService).AssemblyQualifiedName
                },
                    Cache = new CacheConfiguration()
                    {
                        MaxAge = new TimeSpan(0, 5, 0).Ticks,
                        MaxSize = 1000,
                        MaxDirtyAge = new TimeSpan(0, 20, 0).Ticks,
                        MaxPressureAge = new TimeSpan(0, 2, 0).Ticks
                    }
                };

                ApplicationContext.Current.AddServiceProvider(typeof(SQLite.Net.Platform.SqlCipher.SQLitePlatformSqlCipher));

                // Security configuration
                SecurityConfigurationSection secSection = new SecurityConfigurationSection()
                {
                    DeviceName = Environment.MachineName,
                    AuditRetention = new TimeSpan(30, 0, 0, 0, 0)
                };

                // Device key
                //var certificate = X509CertificateUtils.FindCertificate(X509FindType.FindBySubjectName, StoreLocation.LocalMachine, StoreName.My, String.Format("DN={0}.mobile.santedb.org", macAddress));
                //secSection.DeviceSecret = certificate?.Thumbprint;

                // Rest Client Configuration
                ServiceClientConfigurationSection serviceSection = new ServiceClientConfigurationSection()
                {
                    RestClientType = typeof(RestClient)
                };

                // Trace writer
                DiagnosticsConfigurationSection diagSection = new DiagnosticsConfigurationSection()
                {
                    TraceWriter = new System.Collections.Generic.List<TraceWriterConfiguration>() {
                    new TraceWriterConfiguration () {
                        Filter = System.Diagnostics.Tracing.EventLevel.Error,
                        InitializationData = "SanteDB",
                        TraceWriter = new ConsoleTraceWriter (System.Diagnostics.Tracing.EventLevel.Warning, "SanteDB")
                    },
                    new TraceWriterConfiguration() {
                        Filter = System.Diagnostics.Tracing.EventLevel.LogAlways,
                        InitializationData = "SanteDB",
                        TraceWriter = new FileTraceWriter(System.Diagnostics.Tracing.EventLevel.Warning, "SanteDB")
                    }
                }
                };
                retVal.Sections.Add(appletSection);
                retVal.Sections.Add(dataSection);
                retVal.Sections.Add(diagSection);
                retVal.Sections.Add(appSection);
                retVal.Sections.Add(secSection);
                retVal.Sections.Add(serviceSection);
                retVal.Sections.Add(new SynchronizationConfigurationSection()
                {
                    PollInterval = new TimeSpan(0, 5, 0)
                });

                return retVal;
            }
        }

        /// <summary>
        /// Restoration
        /// </summary>
        public SanteDBConfiguration Restore()
        {
            throw new NotImplementedException("Debug environment cannot restore backups");
        }

        /// <summary>
        /// Save the configuation
        /// </summary>
        public void Save(SanteDBConfiguration config)
        {
        }
        
    }
}

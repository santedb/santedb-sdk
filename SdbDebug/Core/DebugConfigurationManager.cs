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
using SanteDB.DisconnectedClient.Xamarin.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.DisconnectedClient.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SdbDebug.Options;
using MohawkCollege.Util.Console.Parameters;
using System.IO;
using SanteDB.DisconnectedClient.Core.Security;
using SanteDB.DisconnectedClient.Xamarin.Net;
using SanteDB.DisconnectedClient.Core.Data.Warehouse;
using SanteDB.DisconnectedClient.Xamarin.Rules;
using SanteDB.DisconnectedClient.Core.Data;
using SanteDB.DisconnectedClient.Core.Caching;
using SanteDB.DisconnectedClient.Xamarin.Threading;
using SanteDB.Core.Protocol;
using SanteDB.Cdss.Xml;
using SanteDB.Core.Services.Impl;
using SanteDB.DisconnectedClient.Xamarin.Services;
using SanteDB.DisconnectedClient.Xamarin.Http;
using SanteDB.DisconnectedClient.Xamarin.Diagnostics;
using SdbDebug.Shell;
using SanteDB.DisconnectedClient.SQLite.Connection;
using SanteDB.DisconnectedClient.SQLite;
using SanteDB.DisconnectedClient.SQLite.Security;
using SanteDB.DisconnectedClient.Core.Services.Impl;

namespace SdbDebug.Core
{
    /// <summary>
    /// Represents a configuration manager which is used for the debugger
    /// </summary>
    public class DebugConfigurationManager : IConfigurationManager
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DebugConfigurationManager));

        // Current configuration
        private SanteDBConfiguration m_configuration;

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
        /// Get the configuration
        /// </summary>
        public SanteDBConfiguration Configuration
        {
            get
            {
                if (this.m_configuration == null)
                    this.Load();
                return this.m_configuration;
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
        public void Backup()
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
        public void Load()
        {
            if (!String.IsNullOrEmpty(this.m_configPath))
                using (var fs = File.OpenRead(this.m_configPath))
                {
                    this.m_configuration = SanteDBConfiguration.Load(fs);
                }
            else
            {
                this.m_configuration = new SanteDBConfiguration();

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
                    typeof(LocalPolicyDecisionService).AssemblyQualifiedName,
                    typeof(SQLitePolicyInformationService).AssemblyQualifiedName,
                    typeof(LocalPatientService).AssemblyQualifiedName,
                    typeof(LocalPlaceService).AssemblyQualifiedName,
                    //typeof(LocalAlertService).AssemblyQualifiedName,
                    typeof(LocalConceptService).AssemblyQualifiedName,
                    typeof(LocalEntityRepositoryService).AssemblyQualifiedName,
                    typeof(LocalOrganizationService).AssemblyQualifiedName,
                    typeof(SQLiteRoleProviderService).AssemblyQualifiedName,
                    typeof(LocalSecurityService).AssemblyQualifiedName,
                    typeof(LocalMaterialService).AssemblyQualifiedName,
                    typeof(LocalBatchService).AssemblyQualifiedName,
                    typeof(LocalActService).AssemblyQualifiedName,
                    typeof(LocalProviderService).AssemblyQualifiedName,
                    typeof(LocalTagPersistenceService).AssemblyQualifiedName,
                    typeof(NetworkInformationService).AssemblyQualifiedName,
                    typeof(BusinessRulesDaemonService).AssemblyQualifiedName,
                    typeof(LocalEntitySource).AssemblyQualifiedName,
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

                appSection.ServiceTypes.Add(typeof(SQLite.Net.Platform.Generic.SQLitePlatformGeneric).AssemblyQualifiedName);

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
                this.m_configuration.Sections.Add(appletSection);
                this.m_configuration.Sections.Add(dataSection);
                this.m_configuration.Sections.Add(diagSection);
                this.m_configuration.Sections.Add(appSection);
                this.m_configuration.Sections.Add(secSection);
                this.m_configuration.Sections.Add(serviceSection);
                this.m_configuration.Sections.Add(new SynchronizationConfigurationSection()
                {
                    PollInterval = new TimeSpan(0, 5, 0)
                });
            }
        }

        /// <summary>
        /// Restoration
        /// </summary>
        public void Restore()
        {
            throw new NotImplementedException("Debug environment cannot restore backups");
        }

        /// <summary>
        /// Save the configuation
        /// </summary>
        public void Save()
        {
        }

        /// <summary>
        /// Save the specified configruation
        /// </summary>
        public void Save(SanteDBConfiguration config)
        {
        }
    }
}

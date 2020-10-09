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
 * Date: 2018-7-4
 */
using SanteDB.Core;
using SanteDB.Core.Applets.Model;
using SanteDB.Core.Applets.Services;
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.DisconnectedClient;
using SanteDB.DisconnectedClient.Backup;
using SanteDB.DisconnectedClient.Configuration;
using SanteDB.DisconnectedClient.Configuration.Data;
using SanteDB.DisconnectedClient.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AppletDebugger
{
    /// <summary>
    /// Test application context
    /// </summary>
    public class MiniApplicationContext : ApplicationContext
    {

        // The application
        private static readonly SanteDB.Core.Model.Security.SecurityApplication c_application = new SanteDB.Core.Model.Security.SecurityApplication()
        {
            ApplicationSecret = "A1CF054D04D04CD1897E114A904E328D",
            Key = Guid.Parse("4C5A581C-A6EE-4267-9231-B0D3D50CC08A"),
            Name = "org.santedb.debug"
        };

        /// <summary>
        /// Show toast
        /// </summary>
        public override void ShowToast(string subject)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("TOAST >>>> {0}", subject);
            Console.ResetColor();
        }

        /// <summary>
        /// Get allowed synchronization modes
        /// </summary>
        public override SynchronizationMode Modes => SynchronizationMode.Online | SynchronizationMode.Sync;

        /// <summary>
        /// Get the application
        /// </summary>
        public override SecurityApplication Application
        {
            get
            {
                return c_application;
            }
        }

        /// <summary>
        /// Static CTOR bind to global handlers to log errors
        /// </summary>
        /// <value>The current.</value>
        static MiniApplicationContext()
        {

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (ApplicationContext.Current != null)
                {
                    Tracer tracer = Tracer.GetTracer(typeof(ApplicationContext));
                    tracer.TraceEvent(EventLevel.Critical, "Uncaught exception: {0}", e.ExceptionObject.ToString());
                }
            };


        }

        /// <summary>
        /// Mini application context
        /// </summary>
        public MiniApplicationContext(String instanceName) : base(new MiniConfigurationManager(instanceName))
        {

        }
        /// <summary>
		/// Starts the application context using in-memory default configuration for the purposes of 
		/// configuring the software
		/// </summary>
		/// <returns><c>true</c>, if temporary was started, <c>false</c> otherwise.</returns>
		public static bool StartTemporary(ConsoleParameters consoleParms)
        {
            try
            {

                // Is autoconfiguration enabled on this 
                var retVal = new MiniApplicationContext(consoleParms.InstanceName);
                retVal.SetProgress("Run setup", 0);

                ApplicationServiceContext.Current = ApplicationContext.Current = retVal;


                retVal.m_tracer = Tracer.GetTracer(typeof(MiniApplicationContext));
                foreach (var tr in retVal.Configuration.GetSection<DiagnosticsConfigurationSection>().TraceWriter)
                {
                    Tracer.AddWriter(Activator.CreateInstance(tr.TraceWriter, tr.Filter, tr.InitializationData) as TraceWriter, tr.Filter);
                }

                retVal.SetProgress("Loading configuration", 0.2f);
                var appService = retVal.GetService<IAppletManagerService>();

                if (consoleParms.References != null)
                {
                    // Load references
                    foreach (var appletInfo in consoleParms.References)// Directory.GetFiles(this.m_configuration.GetSection<AppletConfigurationSection>().AppletDirectory)) {
                        try
                        {
                            retVal.m_tracer.TraceInfo("Loading applet {0}", appletInfo);

                            String appletPath = appletInfo;

                            // Is there a pak extension?
                            if (Path.GetExtension(appletPath) != ".pak")
                                appletPath += ".pak";

                            if (!Path.IsPathRooted(appletInfo))
                                appletPath = Path.Combine(Environment.CurrentDirectory, appletPath);

                            // Does the reference exist in the current directory?
                            if (!File.Exists(appletPath))
                                appletPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.GetFileName(appletPath));

                            Console.WriteLine("Including {0}...", appletPath);
                            using (var fs = File.OpenRead(appletPath))
                            {

                                var package = AppletPackage.Load(fs);
                                if (package is AppletSolution)
                                {
                                    // Look for other applets with this 
                                    foreach (var itm in (package as AppletSolution).Include)
                                    {
                                        retVal.m_tracer.TraceInfo("Loading solution content project {0}", itm.Meta.Id);
                                        appService.LoadApplet(itm.Unpack());
                                    }
                                }
                                else
                                {
                                    retVal.m_tracer.TraceInfo("Loading {0} v{1}", package.Meta.Id, package.Meta.Version);
                                    // Is this applet in the allowed applets
                                    appService.LoadApplet(package.Unpack());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            retVal.m_tracer.TraceError("Loading applet {0} failed: {1}", appletInfo, e.ToString());
                            throw;
                        }
                }

                // Does openiz.js exist as an asset?
                var oizJs = appService.Applets.ResolveAsset("/org.santedb.core/js/santedb.js");

                // Load all solution manifests and attempt to find their pathspec
                if (!String.IsNullOrEmpty(consoleParms.SolutionFile))
                {
                    using (var fs = File.OpenRead(consoleParms.SolutionFile))
                    {
                        retVal.m_tracer.TraceInfo("Will load solution : {0}", consoleParms.SolutionFile);
                        var solution = AppletManifest.Load(fs);

                        // Load include elements
                        var solnDir = Path.GetDirectoryName(consoleParms.SolutionFile);

                        // Preload any manifests
                        var refManifests = new Dictionary<String, AppletManifest>();
                        // Load reference manifests
                        foreach (var dir in Directory.GetDirectories(solnDir))
                        {
                            try
                            {
                                var appletFile = Path.Combine(dir, "manifest.xml");
                                if (File.Exists(appletFile))
                                    using (var manifestStream = File.OpenRead(appletFile))
                                        refManifests.Add(dir, AppletManifest.Load(manifestStream));
                            }
                            catch { }
                        }

                        // Load dependencies
                        foreach (var dep in solution.Info.Dependencies)
                        {
                            // Attempt to load the appropriate manifest file
                            var cand = refManifests.FirstOrDefault(o => o.Value.Info.Id == dep.Id);
                            if (cand.Value != null)
                            {
                                retVal.m_tracer.TraceInfo("Loading applet {0} from {1}", dep.Id, cand.Key);
                                (appService as MiniAppletManagerService).m_appletBaseDir.Add(cand.Value, cand.Key);
                                // Is this applet in the allowed applets

                                // public key token match?
                                appService.LoadApplet(cand.Value);
                            }
                        }
                    }
                }


                // Load all user-downloaded applets in the data directory
                if(consoleParms.AppletDirectories != null)
                    foreach (var appletDir in consoleParms.AppletDirectories)// Directory.GetFiles(this.m_configuration.GetSection<AppletConfigurationSection>().AppletDirectory)) {
                        try
                        {
                            if (!Directory.Exists(appletDir) || !File.Exists(Path.Combine(appletDir, "manifest.xml")))
                                continue;

                            retVal.m_tracer.TraceInfo("Loading applet {0}", appletDir);
                            String appletPath = Path.Combine(appletDir, "manifest.xml");
                            using (var fs = File.OpenRead(appletPath))
                            {
                                AppletManifest manifest = AppletManifest.Load(fs);
                                (appService as MiniAppletManagerService).m_appletBaseDir.Add(manifest, appletDir);
                                // Is this applet in the allowed applets

                                // public key token match?
                                appService.LoadApplet(manifest);
                            }
                        }
                        catch (Exception e)
                        {
                            retVal.m_tracer.TraceError("Loading applet {0} failed: {1}", appletDir, e.ToString());
                            throw;
                        }

                if (oizJs?.Content != null)
                {
                    byte[] content = appService.Applets.RenderAssetContent(oizJs);
                    var oizJsStr = Encoding.UTF8.GetString(content, 0, content.Length);
                    oizJs.Content = oizJsStr + (appService as MiniAppletManagerService).GetShimMethods();
                }

                if(!consoleParms.Restore)
                    retVal.GetService<IThreadPoolService>().QueueUserWorkItem((o) => retVal.Start());

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SanteDB FATAL: {0}", e.ToString());
                return false;
            }
        }


        /// <summary>
        /// Start the application context
        /// </summary>
        public static bool Start(ConsoleParameters consoleParms)
        {

            var retVal = new MiniApplicationContext(consoleParms.InstanceName);

            // Not configured
            if (!retVal.ConfigurationPersister.IsConfigured)
            {
                return false;
            }
            else
            { // load configuration
                try
                {
                    // Set master application context
                    ApplicationServiceContext.Current = ApplicationContext.Current = retVal;
                    retVal.ConfigurationPersister.Backup(retVal.Configuration);

                    retVal.AddServiceProvider(typeof(DefaultBackupService));
                    retVal.GetService<IBackupService>().AutoRestore();

                    retVal.m_tracer = Tracer.GetTracer(typeof(MiniApplicationContext));
                    foreach (var tr in retVal.Configuration.GetSection<DiagnosticsConfigurationSection>().TraceWriter)
                    {
                        Tracer.AddWriter(Activator.CreateInstance(tr.TraceWriter, tr.Filter, tr.InitializationData) as TraceWriter, tr.Filter);
                    }
                    var appService = retVal.GetService<IAppletManagerService>();

                    retVal.SetProgress("Loading configuration", 0.2f);

                    if (consoleParms.References != null)
                    {
                        // Load references
                        foreach (var appletInfo in consoleParms.References)// Directory.GetFiles(this.m_configuration.GetSection<AppletConfigurationSection>().AppletDirectory)) {
                            try
                            {
                                retVal.m_tracer.TraceInfo("Loading applet {0}", appletInfo);
                                String appletPath = appletInfo;
                                if (!Path.IsPathRooted(appletInfo))
                                    appletPath = Path.Combine(Environment.CurrentDirectory, appletPath);
                                using (var fs = File.OpenRead(appletPath))
                                {

                                    var package = AppletPackage.Load(fs);
                                    if (package is AppletSolution)
                                    {
                                        // Look for other applets with this 
                                        foreach (var itm in (package as AppletSolution).Include)
                                        {
                                            retVal.m_tracer.TraceInfo("Loading solution content project {0}", itm.Meta.Id);
                                            appService.LoadApplet(itm.Unpack());
                                        }
                                    }
                                    else
                                    {
                                        retVal.m_tracer.TraceInfo("Loading {0} v{1}", package.Meta.Id, package.Meta.Version);
                                        // Is this applet in the allowed applets
                                        appService.LoadApplet(package.Unpack());
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                retVal.m_tracer.TraceError("Loading applet {0} failed: {1}", appletInfo, e.ToString());
                                throw;
                            }
                    }

                    // Does openiz.js exist as an asset?
                    var oizJs = appService.Applets.ResolveAsset("/org.santedb.core/js/santedb.js");

                    // Load all solution manifests and attempt to find their pathspec
                    if (!String.IsNullOrEmpty(consoleParms.SolutionFile))
                    {
                        using (var fs = File.OpenRead(consoleParms.SolutionFile))
                        {
                            retVal.m_tracer.TraceInfo("Will load solution : {0}", consoleParms.SolutionFile);
                            var solution = AppletManifest.Load(fs);

                            // Load include elements
                            var solnDir = Path.GetDirectoryName(consoleParms.SolutionFile);

                            // Preload any manifests
                            var refManifests = new Dictionary<String, AppletManifest>();
                            // Load reference manifests
                            foreach (var dir in Directory.GetDirectories(solnDir))
                            {
                                try
                                {
                                    var appletFile = Path.Combine(dir, "manifest.xml");
                                    if (File.Exists(appletFile))
                                        using (var manifestStream = File.OpenRead(appletFile))
                                            refManifests.Add(dir, AppletManifest.Load(manifestStream));
                                }
                                catch { }
                            }

                            // Load dependencies
                            foreach (var dep in solution.Info.Dependencies)
                            {
                                // Attempt to load the appropriate manifest file
                                var cand = refManifests.FirstOrDefault(o => o.Value.Info.Id == dep.Id);
                                if (cand.Value != null)
                                {
                                    retVal.m_tracer.TraceInfo("Loading applet {0} from {1}", dep.Id, cand.Key);
                                    (appService as MiniAppletManagerService).m_appletBaseDir.Add(cand.Value, cand.Key);
                                    // Is this applet in the allowed applets

                                    // public key token match?
                                    appService.LoadApplet(cand.Value);
                                }
                            }
                        }
                    }


                    // Load all user-downloaded applets in the data directory
                if(consoleParms.AppletDirectories != null)
                        foreach (var appletDir in consoleParms.AppletDirectories)// Directory.GetFiles(this.m_configuration.GetSection<AppletConfigurationSection>().AppletDirectory)) {
                        try
                        {
                            if (!Directory.Exists(appletDir) || !File.Exists(Path.Combine(appletDir, "manifest.xml")))
                                continue;

                            retVal.m_tracer.TraceInfo("Loading applet {0}", appletDir);
                            String appletPath = Path.Combine(appletDir, "manifest.xml");
                            using (var fs = File.OpenRead(appletPath))
                            {
                                AppletManifest manifest = AppletManifest.Load(fs);
                                (appService as MiniAppletManagerService).m_appletBaseDir.Add(manifest, appletDir);
                                // Is this applet in the allowed applets

                                // public key token match?
                                appService.LoadApplet(manifest);
                            }
                        }
                        catch (Exception e)
                        {
                            retVal.m_tracer.TraceError("Loading applet {0} failed: {1}", appletDir, e.ToString());
                            throw;
                        }

                    if (oizJs?.Content != null)
                    {
                        byte[] content = appService.Applets.RenderAssetContent(oizJs);
                        var oizJsStr = Encoding.UTF8.GetString(content, 0, content.Length);
                        oizJs.Content = oizJsStr + (appService as MiniAppletManagerService).GetShimMethods();
                    }

                    // Set the entity source
                    EntitySource.Current = new EntitySource(retVal.GetService<IEntitySourceProvider>());

                    // Ensure data migration exists
                    var hasDatabase = retVal.ConfigurationManager.Configuration.GetSection<DcDataConfigurationSection>().ConnectionString.Count > 0;
                        try
                        {
                            // If the DB File doesn't exist we have to clear the migrations
                            if (!File.Exists(retVal.ConfigurationManager.GetConnectionString(retVal.Configuration.GetSection<DcDataConfigurationSection>().MainDataSourceConnectionStringName).GetComponent("dbfile")))
                            {
                                retVal.m_tracer.TraceWarning("Can't find the SanteDB database, will re-install all migrations");
                                retVal.Configuration.GetSection<DcDataConfigurationSection>().MigrationLog.Entry.Clear();
                            }
                            retVal.SetProgress("Migrating databases", 0.6f);

                            ConfigurationMigrator migrator = new ConfigurationMigrator();
                            migrator.Ensure(hasDatabase);

                            // Prepare clinical protocols
                            //retVal.GetService<ICarePlanService>().Repository = retVal.GetService<IClinicalProtocolRepositoryService>();

                        }
                        catch (Exception e)
                        {
                            retVal.m_tracer.TraceError(e.ToString());
                            throw;
                        }
                        finally
                        {
                            retVal.ConfigurationPersister.Save(retVal.Configuration);
                        }


                    if (!retVal.Configuration.GetSection<DiagnosticsConfigurationSection>().TraceWriter.Any(o => o.TraceWriterClassXml.Contains("Console")))
                        retVal.Configuration.GetSection<DiagnosticsConfigurationSection>().TraceWriter.Add(new TraceWriterConfiguration()
                        {
                            TraceWriter = typeof(ConsoleTraceWriter),
#if DEBUG
                            Filter = EventLevel.Informational
#else
                            Filter = EventLevel.Warning
#endif
                        });



                    // Start daemons
                    retVal.GetService<IThreadPoolService>().QueueUserWorkItem((o) => retVal.Start());


                }
                catch (Exception e)
                {
                    retVal.m_tracer?.TraceError(e.ToString());
                    //ApplicationContext.Current = null;
                    throw;
                }
                return true;
            }
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        public override void Exit()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Confirmation
        /// </summary>
        public override bool Confirm(string confirmText)
        {
            string result = "-";
            while ("yn".IndexOf(result, StringComparison.OrdinalIgnoreCase) == -1)
            {
                Console.Write("{0} (Y/N):", confirmText);
                result = Console.ReadKey().KeyChar.ToString();
            }
            return "y".Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Show an alert
        /// </summary>
        public override void Alert(string alertText)
        {
            Console.WriteLine("!!!!{0}!!!!", alertText);
        }

        /// <summary>
        /// Performance log!
        /// </summary>
        public override void PerformanceLog(string className, string methodName, string tagName, TimeSpan counter)
        {
        }

        /// <summary>
        /// Get current context key -- Since miniims is debuggable this is not needed
        /// </summary>
        public override byte[] GetCurrentContextSecurityKey()
        {
            return null;
        }

        /// <summary>
        /// Get all types
        /// </summary>
        public override IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(a =>
            {
                try
                {
                    return a.ExportedTypes;
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            });
        }
    }
}

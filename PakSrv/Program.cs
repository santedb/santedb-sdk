using MohawkCollege.Util.Console.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PakSrv
{
    /// <summary>
    /// PakSrv is a PakMan remote repository server
    /// </summary>
    public class Program
    {
        /// <summary>
        ///  Main program entry point
        /// </summary>
        static void Main(string[] args)
        {

            var parser = new ParameterParser<PakSrvParameters>();

            // Parse parameters
            var parms = parser.Parse(args);
            Trace.TraceInformation("Starting Packaging Server");

            try
            {
                if (parms.Install)
                {
                    string serviceName = $"sdb-pkg-srvr";
                    if (!ServiceTools.ServiceInstaller.ServiceIsInstalled(serviceName))
                    {
                        String argList = String.Empty;
                      
                        ServiceTools.ServiceInstaller.Install(
                            serviceName, $"SanteDB Package Server",
                            $"{Assembly.GetEntryAssembly().Location} {argList}",
                            null, null, ServiceTools.ServiceBootFlag.AutoStart);
                    }
                    else
                        throw new InvalidOperationException("Service instance already installed");
                }
                else if (parms.Uninstall)
                {
                    string serviceName = $"sdb-pkg-srvr";
                    if (ServiceTools.ServiceInstaller.ServiceIsInstalled(serviceName))
                        ServiceTools.ServiceInstaller.Uninstall(serviceName);
                    else
                        throw new InvalidOperationException("Service instance not installed");
                }
                else if (parms.Console)
                {
                    var pakSrv = new PakSrvHost();
                    pakSrv.Start();
                    ManualResetEvent stopEvent = new ManualResetEvent(false);
                    Console.CancelKeyPress += (o, e) => stopEvent.Set();
                    Console.WriteLine("Press CTRL+C key to close...");
                    stopEvent.WaitOne();
                    pakSrv.Stop();
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new PakSrvService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.TraceError("011 899 981 199 911 9725 3!!! {0}", e.ToString());
                Console.WriteLine("011 899 981 199 911 9725 3!!! {0}", e.ToString());

#else
                Trace.TraceError("Error encountered: {0}. Will terminate", e.Message);
                EventLog.WriteEntry("SanteDB Gateway", $"Fatal service error: {e}", EventLogEntryType.Error, 911);
#endif
                Environment.Exit(911);
            }
        }
    }
}

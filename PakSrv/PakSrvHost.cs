using RestSrvr;
using RestSrvr.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PakSrv
{
    /// <summary>
    /// Package service host
    /// </summary>
    public class PakSrvHost
    {

        /// <summary>
        /// The service host
        /// </summary>
        private RestService m_serviceHost = null;

        // Configuration
        private PakSrvConfiguration m_configuration;

        /// <summary>
        /// Start the service host
        /// </summary>
        public void Start()
        {
            using (var fs = File.OpenRead(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "paksrv.config")))
                this.m_configuration = PakSrvConfiguration.Load(fs);

            this.m_serviceHost = new RestService(typeof(PakSrvBehavior));

            foreach(var bind in this.m_configuration.Bindings)
                this.m_serviceHost.AddServiceEndpoint(new Uri(bind), typeof(IPakSrvContract), new RestHttpBinding());
            this.m_serviceHost.AddServiceBehavior(new PakSrvAuthenticationBehavior(this.m_configuration));
            this.m_serviceHost.Start();
        }


        /// <summary>
        /// Stop the rest service
        /// </summary>
        public void Stop ()
        {
            this.m_serviceHost?.Stop();
            this.m_serviceHost = null;
        }


    }
}

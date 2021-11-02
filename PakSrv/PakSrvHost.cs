using RestSrvr;
using RestSrvr.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
        internal static PakSrvConfiguration m_configuration;

        /// <summary>
        /// Start the service host
        /// </summary>
        public void Start()
        {
            var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "paksrv.config");
            if (!File.Exists(configFile))
            {
                m_configuration = new PakSrvConfiguration()
                {
                    Bindings = new List<string>()
                    {
                        "http://0.0.0.0:6039/paksrv"
                    }
                };

                using (var fs = File.Create(configFile))
                    m_configuration.Save(fs);
            }
            else using (var fs = File.OpenRead(configFile))
                    m_configuration = PakSrvConfiguration.Load(fs);

            this.m_serviceHost = new RestService(typeof(PakSrvBehavior));

            foreach (var bind in m_configuration.Bindings)
                this.m_serviceHost.AddServiceEndpoint(new Uri(bind), typeof(IPakSrvContract), new RestHttpBinding());
            this.m_serviceHost.AddServiceBehavior(new PakSrvAuthenticationBehavior(m_configuration));
            this.m_serviceHost.Start();
        }


        /// <summary>
        /// Stop the rest service
        /// </summary>
        public void Stop()
        {
            this.m_serviceHost?.Stop();
            this.m_serviceHost = null;
        }


    }
}

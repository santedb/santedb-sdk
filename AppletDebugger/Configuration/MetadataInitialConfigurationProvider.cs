using SanteDB.Core;
using SanteDB.Core.Configuration;
using SanteDB.DisconnectedClient.Ags.Behaviors;
using SanteDB.DisconnectedClient.Ags.Configuration;
using SanteDB.DisconnectedClient.Configuration;
using SanteDB.Messaging.Metadata;
using SanteDB.Messaging.Metadata.Configuration;
using SanteDB.Messaging.Metadata.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppletDebugger.Configuration
{
    /// <summary>
    /// Swagger initial configuration provider
    /// </summary>
    public class MetadataInitialConfigurationProvider : IInitialConfigurationProvider
    {
        /// <summary>
        /// Get the default configuration for this service
        /// </summary>
        public SanteDBConfiguration Provide(SanteDBConfiguration existing)
        {

            // Add swagger configuration
            if (!existing.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(MetadataMessageHandler)))
            {
                existing.AddSection(new MetadataConfigurationSection()
                {
                    Services = new List<SanteDB.Core.Interop.ServiceEndpointOptions>()
                });
                existing.GetSection<AgsConfigurationSection>().Services.Add(new AgsServiceConfiguration(typeof(MetadataServiceBehavior))
                {
                    Behaviors = new List<AgsBehaviorConfiguration>()
                {
                    new AgsBehaviorConfiguration(typeof(AgsErrorHandlerServiceBehavior))
                },
                    Endpoints = new List<AgsEndpointConfiguration>()
                {
                    new AgsEndpointConfiguration()
                            {
                                Address = "http://127.0.0.1:9200/api-docs",
                                Behaviors = new List<AgsBehaviorConfiguration>() {
                                    new AgsBehaviorConfiguration(typeof(AgsSerializationEndpointBehavior))
                                },
                                Contract = typeof(IMetadataServiceContract)
                            }
                }
                });
            }
            return existing;
        }
    }
}

using PakMan.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PakSrv
{
    /// <summary>
    /// A specialized package manager configuration for the server host
    /// </summary>
    [XmlType(nameof(PakSrvConfiguration), Namespace = "http://santedb.org/pakman")]
    [XmlRoot(nameof(PakSrvConfiguration), Namespace = "http://santedb.org/pakman")]
    public class PakSrvConfiguration 
    {

        // Serializer instance 
        private static readonly XmlSerializer s_serializer = new XmlSerializer(typeof(PakSrvConfiguration));

        /// <summary>
        /// Bindigs for the service host
        /// </summary>
        [XmlArray("binding"), XmlArrayItem("add")]
        public List<String> Bindings { get; set; }

        /// <summary>
        /// Authorization application keys
        /// </summary>
        [XmlArray("authorizations"), XmlArrayItem("add")]
        public List<PakSrvAuthentication> AuthorizedKeys { get; set; }

        /// <summary>
        /// Load the configuration from the specifed source
        /// </summary>
        public static PakSrvConfiguration Load(Stream source)
        {
            return s_serializer.Deserialize(source) as PakSrvConfiguration;
        }

        /// <summary>
        /// Save the configuration file
        /// </summary>
        public void Save(Stream destination)
        {
            s_serializer.Serialize(destination, this);
        }

    }
}

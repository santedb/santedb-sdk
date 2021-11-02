using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PakMan.Configuration
{
    /// <summary>
    /// Pakman configuration
    /// </summary>
    [XmlType(nameof(PakManConfig), Namespace = "http://santedb.org/pakman")]
    [XmlRoot(nameof(PakManConfig), Namespace = "http://santedb.org/pakman")]
    public class PakManConfig
    {

        // Serializer
        private static XmlSerializer s_serializer = new XmlSerializer(typeof(PakManConfig));

        /// <summary>
        /// Repositories
        /// </summary>
        [XmlArray("repositories"), XmlArrayItem("add")]
        public List<PackageRepositoryConfig> Repository { get; set; }


        /// <summary>
        /// Load the specified configuration
        /// </summary>
        public static PakManConfig Load(Stream config)
        {
            return s_serializer.Deserialize(config) as PakManConfig;
        }

        /// <summary>
        /// Save the specified configuration file
        /// </summary>
        public void Save(Stream fs)
        {
            s_serializer.Serialize(fs, this);
        }
    }
}

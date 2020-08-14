using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Repositories
        /// </summary>
        [XmlArray("repositories"), XmlArrayItem("add")]
        public List<String> Repository { get; set; }

    }
}

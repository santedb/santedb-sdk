using System.Xml.Serialization;

namespace PakSrv
{

    /// <summary>
    /// Package service authentication
    /// </summary>
    [XmlType(nameof(PakSrvAuthentication), Namespace = "http://santedb.org/pakman")]
    public class PakSrvAuthentication
    {
        /// <summary>
        /// Gets or sets the principal name
        /// </summary>
        [XmlAttribute("name")]
        public string PrincipalName { get; set; }

        /// <summary>
        /// The secret
        /// </summary>
        [XmlAttribute("secret")]
        public string PrincipalSecret { get; set; }

    }
}
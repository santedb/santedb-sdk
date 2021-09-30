using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FakeDataGenerator
{
    /// <summary>
    /// Represents seed data
    /// </summary>
    [XmlType(nameof(SeedData), Namespace = "http://santedb.org/seed")]
    [XmlRoot(nameof(SeedData), Namespace = "http://santedb.org/seed")]
    public class SeedData
    {

        /// <summary>
        /// Family names
        /// </summary>
        [XmlElement("familyName")]
        public List<String> FamilyNames { get; set; }

        /// <summary>
        /// Load seed data from the specified stream
        /// </summary>
        internal static SeedData Load(Stream stream)
        {
            return (SeedData)new XmlSerializer(typeof(SeedData)).Deserialize(stream);
        }

        /// <summary>
        /// Given names
        /// </summary>
        [XmlElement("givenName")]
        public List<GivenNameSeedData> GivenNames { get; set; }

        /// <summary>
        /// Name of streets
        /// </summary>
        [XmlElement("streetName")]
        public List<String> Streets { get; set; }

        /// <summary>
        /// Name of cities
        /// </summary>
        [XmlElement("city")]
        public List<String> Cities { get; set; }

        /// <summary>
        /// The name of states
        /// </summary>
        [XmlElement("state")]
        public List<String> States { get; set; }

    }

    /// <summary>
    /// Give Name Seed data
    /// </summary>
    [XmlType(nameof(GivenNameSeedData), Namespace = "http://santedb.org/seed")]
    public class GivenNameSeedData
    {
        /// <summary>
        /// The gender of the patient
        /// </summary>
        [XmlAttribute("gender")]
        public String Gender { get; set; }

        /// <summary>
        /// The value of the name
        /// </summary>
        [XmlText]
        public String Value { get; set; }

        public override string ToString() => this.Value;
    }
}

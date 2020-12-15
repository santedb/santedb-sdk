using PakMan.Repository;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace PakMan.Configuration
{
    /// <summary>
    /// Package repository configuratio
    /// </summary>
    [XmlType(nameof(PackageRepositoryConfig), Namespace = "http://santedb.org/pakman")]
    public class PackageRepositoryConfig
    {

        // Repository
        private IPackageRepository m_repository;

        /// <summary>
        /// Any attribute on the configuration
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] Configuration { get; set; }

        /// <summary>
        /// Gets or sets the path configured
        /// </summary>
        [XmlText]
        public string Path { get; set; }

        /// <summary>
        /// Gets the repository instance
        /// </summary>
        public IPackageRepository GetRepository()
        {

            if (this.m_repository == null)
            {
                this.m_repository = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.ExportedTypes)
                    .Where(t => !t.IsInterface && typeof(IPackageRepository).IsAssignableFrom(t) && !t.IsAbstract)
                    .Select(t => Activator.CreateInstance(t) as IPackageRepository)
                    .FirstOrDefault(o => o.Scheme == new Uri(this.Path).Scheme);
                this.m_repository.Initialize(new Uri(this.Path), this.Configuration?.ToDictionary(o=>o.Name, o=>o.Value));
            }

            return this.m_repository;

        }

    }
}
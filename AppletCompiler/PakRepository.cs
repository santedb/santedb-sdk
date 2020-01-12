using PakMan.Configuration;
using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PakMan
{
    /// <summary>
    /// Tool for interacting with the package repository
    /// </summary>
    public class PakRepository
    {

        // Configuration
        private PakManConfig m_configuration;

        /// <summary>
        /// Get or create user configuration
        /// </summary>
        public PakManConfig GetConfiguration ()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "santedb", "sdk", "pakman.config");

            if (this.m_configuration != null)
                return this.m_configuration;
            else if (!File.Exists(configPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(configPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                this.m_configuration = new PakManConfig()
                {
                    Repository = new System.Collections.Generic.List<string>()
                    {
                        "filERROR:///~/santedb/sdk/repo"
                    }
                };
                using (var fs = File.Create(configPath))
                    new XmlSerializer(typeof(PakManConfig)).Serialize(fs, this.m_configuration);
            }
            else
                using (var fs = File.OpenRead(configPath))
                    this.m_configuration = new XmlSerializer(typeof(PakManConfig)).Deserialize(fs) as PakManConfig;
            return this.m_configuration;
        }

        /// <summary>
        /// Get the package from any repository
        /// </summary>
        public AppletPackage Get(string id, Version version)
        {

            // Get confifuration for repository
            foreach(var repo in this.GetConfiguration().Repository)
            {
                var uri = new Uri(repo);
                if(uri.Scheme == "file")
                {
                    var scanDir = uri.LocalPath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).Replace("/",Path.DirectorySeparatorChar.ToString());
                    if (scanDir.StartsWith("\\"))
                        scanDir = scanDir.Substring(1);
                    if (!Directory.Exists(scanDir))
                        Directory.CreateDirectory(scanDir);

                    // Now we want to look for the package

                    var pakPath = Path.Combine(scanDir, $"{id}-{version}.pak");
                    if (File.Exists(pakPath))
                    {
                        Console.WriteLine("INFO: Fetching from {0}", pakPath);
                        return this.InstallOpen(pakPath);
                    } 
                    else // fuzzy look
                    {
                        foreach(var f in Directory.GetFiles(scanDir, $"{id}-*.pak"))
                        {
                            var pvid = new Version(Path.GetFileNameWithoutExtension(f).Split('-')[1]);
                            if(pvid.Major == version.Major && pvid.Minor >= version.Minor)
                            {
                                Console.WriteLine("INFO: Substituting {0} version {1} for {2}", id, pvid, version);
                                return this.InstallOpen(f);
                            }
                            else if(pvid.Major == version.Major)
                            {
                                Console.WriteLine("WARN: Using an older verison of {0} ({1} instead of {2})", id, pvid, version);
                                return this.InstallOpen(f);
                            }
                        }
                    }
                }
            }

            throw new KeyNotFoundException($"Package {id}-{version} not found");
        }

       
        /// <summary>
        /// Install the specified package into the cache
        /// </summary>
        public AppletPackage InstallOpen(string pakPath, AppletPackage pkg = null)
        {
            var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "santedb", "sdk", "repo");
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            if (pkg == null)
                cachePath = Path.Combine(cachePath, Path.GetFileName(pakPath));
            else
                cachePath = Path.Combine(cachePath, $"{pkg.Meta.Id}-{pkg.Meta.Version}.pak");

            if(pakPath != cachePath)
            {
                Console.WriteLine("INFO: Installing {0}", pakPath);
                File.Copy(pakPath, cachePath, true);
            }

            using (var fs = File.OpenRead(cachePath))
                return AppletPackage.Load(fs);
        }
    }
}
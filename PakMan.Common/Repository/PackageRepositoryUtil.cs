﻿using PakMan.Configuration;
using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PakMan.Repository
{
    /// <summary>
    /// Package repository utility
    /// </summary>
    public static class PackageRepositoryUtil
    {

        /// <summary>
        /// Configuration
        /// </summary>
        private static PakManConfig s_configuration;

        // Pseudo configuration for local cache
        private static PackageRepositoryConfig s_localCache;

        // Sante DB SDK
        private const string LocalCachePath = "file:///~/.santedb-sdk";

        /// <summary>
        /// Static ctor
        /// </summary>
        static PackageRepositoryUtil ()
        {
            using(var fs = System.IO.File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "santedb","sdk","pakman.config")))
            {
                s_configuration = PakManConfig.Load(fs);
            }

            s_localCache = s_configuration.Repository.Find(o => o.Path == LocalCachePath);
            if (s_localCache == null)
            {
                s_localCache = new PackageRepositoryConfig()
                {
                    Path = LocalCachePath
                };
                s_configuration.Repository.Add(s_localCache);
            }
        }

        /// <summary>
        /// Get specified package from any package repository
        /// </summary>
        public static AppletPackage GetFromAny(String packageId, Version packageVersion)
        {

            AppletPackage retVal = null;

            foreach (var rep in s_configuration.Repository)
            {
                try
                {
                    retVal = rep.GetRepository().Get(packageId, packageVersion);

                    if (retVal.Version == packageVersion.ToString())
                        break;
                }
                catch
                {
                    
                }
            }
            return retVal;

        }

        /// <summary>
        /// Install the package into the local cache repository
        /// </summary>
        public static void InstallCache(AppletPackage pkg)
        {
            s_localCache.GetRepository().Put(pkg);
        }
    }
}

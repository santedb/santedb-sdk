﻿using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PakMan.Repository
{
    /// <summary>
    /// Represents a package repository management
    /// </summary>
    public interface IPackageRepository
    {

        /// <summary>
        /// Initialize the package repository
        /// </summary>
        /// <param name="basePath">The base path</param>
        /// <param name="configuration">The configuration object for the repository</param>
        void Initialize(Uri basePath, IDictionary<String, String> configuration);

        /// <summary>
        /// Gets the scheme of this package repository
        /// </summary>
        String Scheme { get; }

        /// <summary>
        /// Gets a specific version of the package from the package repository
        /// </summary>
        /// <param name="id">The id of the package</param>
        /// <param name="version">The version of the package to retrieve</param>
        /// <param name="exactVersion">When true, the package must be the exact version</param>
        /// <returns>The applet package contents</returns>
        AppletPackage Get(string id, Version version, bool exactVersion = false);

        /// <summary>
        /// Gets all the packages matching the specified query
        /// </summary>
        /// <param name="count">The number of results to retrieve</param>
        /// <param name="offset">The offset of the first result</param>
        /// <param name="query">The query filter to execute</param>
        /// <param name="totalResults">The number of matching results</param>
        IEnumerable<AppletInfo> Find(Expression<Func<AppletInfo, bool>> query, int offset, int count , out int totalResults);

        /// <summary>
        /// Puts a package into the repository
        /// </summary>
        /// <param name="package">The package to be installed</param>
        /// <returns>The installed package</returns>
        AppletInfo Put(AppletPackage package);
    }
}

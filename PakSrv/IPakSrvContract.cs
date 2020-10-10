using RestSrvr.Attributes;
using SanteDB.Core.Applets.Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace PakSrv
{
    /// <summary>
    /// Pak service contract
    /// </summary>
    [ServiceContract(Name = "SanteDB PakServer")]
    public interface IPakSrvContract
    {

        /// <summary>
        /// Query all packages 
        /// </summary>
        /// <returns></returns>
        [Get("pak")]
        List<AppletInfo> Find();

        /// <summary>
        /// Push a package to the package repository
        /// </summary>
        [Post("pak")]
        AppletInfo Put(Stream body);

        /// <summary>
        /// Get package (most recent)
        /// </summary>
        [Get("pak/{id}")]
        Stream Get(string id);

        /// <summary>
        /// Get package specific version
        /// </summary>
        [Get("pak/{id}/{version}")]
        Stream Get(string id, string version);

        /// <summary>
        /// Get the most recent headers for a package
        /// </summary>
        [RestInvoke("HEAD", "pak/{id}")]
        void Head(string id);

        /// <summary>
        /// Delete (unlist a package)
        /// </summary>
        [Delete("pak/{id}")]
        AppletInfo Delete(string id);

        /// <summary>
        /// Delete a specific version of a package
        /// </summary>
        [Delete("pak/{id}/{version}")]
        AppletInfo Delete(string id, string version);
    }
}

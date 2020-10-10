using PakMan.Repository;
using RestSrvr;
using RestSrvr.Attributes;
using SanteDB.Core.Applets.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PakSrv
{
    /// <summary>
    /// Package service behavior
    /// </summary>
    [ServiceBehavior(InstanceMode = ServiceInstanceMode.Singleton)]
    public class PakSrvBehavior : IPakSrvContract
    {
        /// <summary>
        /// Delete the specified package
        /// </summary>
        public AppletInfo Delete(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Delete a specific version of the package
        /// </summary>
        public AppletInfo Delete(string id, string version)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Find a package
        /// </summary>
        public List<AppletInfo> Find()
        {
            var filter = QueryExpressionParser.BuildLinqExpression<AppletInfo>(NameValueCollection.ParseQueryString(RestOperationContext.Current.IncomingRequest.Url.Query));
            string offset = RestOperationContext.Current.IncomingRequest.QueryString["_offset"] ?? "0",
                count = RestOperationContext.Current.IncomingRequest.QueryString["_count"] ?? "10";
            return PackageRepositoryUtil.FindFromAny(filter, Int32.Parse(offset), Int32.Parse(count)).ToList();
        }

        /// <summary>
        /// Get a specific package
        /// </summary>
        public Stream Get(string id)
        {
            MemoryStream retVal = new MemoryStream();
            var pkg = PackageRepositoryUtil.GetFromAny(id, null);
            if (pkg == null)
                throw new KeyNotFoundException($"Pakcage {id} not found");
            this.AddHeaders(pkg);
            pkg.Save(retVal);
            retVal.Seek(0, SeekOrigin.Begin);
            return retVal;
        }

        /// <summary>
        /// Add specified headers to the response stream
        /// </summary>
        private void AddHeaders(AppletPackage pkg)
        {
            RestOperationContext.Current.OutgoingResponse.AppendHeader("ETag", pkg.Meta.Version);
            RestOperationContext.Current.OutgoingResponse.AppendHeader("Location", $"/pkg/{pkg.Meta.Id}/{pkg.Meta.Version}");
            RestOperationContext.Current.OutgoingResponse.AppendHeader("Last-Modified", pkg.Meta.TimeStamp.GetValueOrDefault().ToUniversalTime().ToString("WWW, dd MMM yyyy HH:mm:ss GMT"));
        }

        /// <summary>
        /// Gets a specific version of the package
        /// </summary>
        public Stream Get(string id, string version)
        {
            MemoryStream retVal = new MemoryStream();
            var pkg = PackageRepositoryUtil.GetFromAny(id, new System.Version(version));
            if (pkg == null)
                throw new KeyNotFoundException($"Package {id} verison {version} not found");
            this.AddHeaders(pkg);
            pkg.Save(retVal);
            retVal.Seek(0, SeekOrigin.Begin);
            return retVal;
        }

        /// <summary>
        /// Return only headers from the specifed version
        /// </summary>
        /// <param name="id"></param>
        public void Head(string id)
        {
            var pkg = PackageRepositoryUtil.GetFromAny(id, null);
            if (pkg == null)
                throw new KeyNotFoundException($"Package {id} not found");
            this.AddHeaders(pkg);
        }

        public AppletInfo Put(Stream body)
        {
            throw new System.NotImplementedException();
        }
    }
}
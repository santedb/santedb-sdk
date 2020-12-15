using PakMan.Repository;
using RestSrvr;
using RestSrvr.Attributes;
using RestSrvr.Exceptions;
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
        /// Configuration
        /// </summary>
        private PakSrvConfiguration m_configuration;

        /// <summary>
        /// Configuration
        /// </summary>
        public PakSrvBehavior()
        {
            this.m_configuration = PakSrvHost.m_configuration;
        }
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
            return this.m_configuration.Repository.GetRepository().Find(filter, Int32.Parse(offset), Int32.Parse(count), out int _).ToList();
        }

        /// <summary>
        /// Get a specific package
        /// </summary>
        public Stream Get(string id)
        {
            MemoryStream retVal = new MemoryStream();
            var pkg = this.m_configuration.Repository.GetRepository().Get(id, null);
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
            var pkg = this.m_configuration.Repository.GetRepository().Get(id, new System.Version(version), true);
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
            var pkg = this.m_configuration.Repository.GetRepository().Get(id, null);
            if (pkg == null)
                throw new KeyNotFoundException($"Package {id} not found");
            this.AddHeaders(pkg);
        }

        /// <summary>
        /// Put the application into the file repository
        /// </summary>
        public AppletInfo Put(Stream body)
        {
            var package = AppletPackage.Load(body);

            try
            {
                this.m_configuration.Repository.GetRepository().Get(package.Meta.Id, new Version(package.Meta.Version), true);
                throw new FaultException(409, $"Package {package.Meta.Id} version {package.Meta.Version} already exists");
            }
            catch (KeyNotFoundException)
            {
                return this.m_configuration.Repository.GetRepository().Put(package);
            }
            finally
            {
            }
        }
    }
}
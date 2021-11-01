using RestSrvr;
using RestSrvr.Message;
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace PakSrv
{
    /// <summary>
    /// Service behavior that attaches an authentication behavior
    /// </summary>
    /// TOOD: Refactor this to be more robust
    public class PakSrvAuthenticationBehavior : IServiceBehavior, IServicePolicy
    {
        // Configuration
        private PakSrvConfiguration m_configuration;

        /// <summary>
        /// Create a new package service auth behavior
        /// </summary>
        public PakSrvAuthenticationBehavior(PakSrvConfiguration config)
        {
            this.m_configuration = config;
        }

        /// <summary>
        /// Apply the policy to the request message
        /// </summary>
        public void Apply(RestRequestMessage request)
        {
            var authHeader = request.Headers["Authorization"];

            try
            {
                // Validate the auth header
                if (String.IsNullOrEmpty(authHeader) && !"GET".Equals(request.Method, StringComparison.OrdinalIgnoreCase) && !"HEAD".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
                    throw new SecurityException("Request is not authorized");
                else if (!String.IsNullOrEmpty(authHeader))
                {
                    var tokenized = authHeader.Split(' ');
                    if (!"basic".Equals(tokenized[0], StringComparison.OrdinalIgnoreCase))
                        throw new SecurityException("Invalid authorization scheme");

                    var authData = Encoding.UTF8.GetString(Convert.FromBase64String(tokenized[1])).Split(':');
                    if (!2.Equals(authData.Length))
                        throw new SecurityException("Invalid authorization header");

                    // attempt auth using config
                    var authn = this.Authorize(authData[0], authData[1]);
                    if (authn == null)
                        throw new SecurityException("Authorization failure");

                    RestOperationContext.Current.Data.Add("auth", authn);
                }
            }
            catch (SecurityException)
            {
                RestOperationContext.Current.OutgoingResponse.AddHeader("WWW-Authenticate", "basic");
                throw;
            }
        }

        /// <summary>
        /// Authoriize the data
        /// </summary>
        private PakSrvAuthentication Authorize(string user, string pass)
        {
            var hasher = SHA256.Create();
            var pwdHash = BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pass))).Replace("-", "");
            var userHash = BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user))).Replace("-", "");
            using (var fs = File.OpenText(".access"))
            {
                while (!fs.EndOfStream)
                {
                    var eData = fs.ReadLine().Split(':');
                    if (eData[0].Equals(userHash, StringComparison.OrdinalIgnoreCase) &&
                        eData[1].Equals(pwdHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PakSrvAuthentication()
                        {
                            PrincipalName = user
                        };
                    }
                }
            }
            Trace.TraceError("Access denied for {0} : {1}:{2}", user, userHash, pwdHash);
            throw new SecurityException($"User {user} invalid");
        }

        /// <summary>
        /// Apply the service behavior
        /// </summary>
        public void ApplyServiceBehavior(RestService service, ServiceDispatcher dispatcher)
        {
            dispatcher.AddServiceDispatcherPolicy(this);
        }
    }
}
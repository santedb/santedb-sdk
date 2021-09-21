using RestSrvr;
using RestSrvr.Message;
using System;
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

                    var hashData = BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(authData[1]))).Replace("-", "").ToUpper();

                    // attempt auth using config
                    var authn = this.m_configuration.AuthorizedKeys.Find(o => authData[0].Equals(o.PrincipalName, StringComparison.OrdinalIgnoreCase) && hashData.Equals(o.PrincipalSecret, StringComparison.OrdinalIgnoreCase));
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
        /// Apply the service behavior
        /// </summary>
        public void ApplyServiceBehavior(RestService service, ServiceDispatcher dispatcher)
        {
            dispatcher.AddServiceDispatcherPolicy(this);
        }
    }
}
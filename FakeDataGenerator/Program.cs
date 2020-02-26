using MARC.Everest.Threading;
using MohawkCollege.Util.Console.Parameters;
using SanteDB.Core.Http;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.DisconnectedClient.Xamarin.Http;
using SanteDB.DisconnectedClient.Xamarin.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeDataGenerator
{
    /// <summary>
    /// Fake data generator which can generate massive amounts of fake patients and simulate heavy traffic loads
    /// </summary>
    class Program
    {

        // Seed data
        static SeedData s_seedData;

        // Random
        static Random s_random;

        // Authority key
        static Guid? s_authorityKey;


        static void Main(string[] args)
        {

            s_random = new Random();
            s_seedData = SeedData.Load(typeof(Program).Assembly.GetManifestResourceStream("FakeDataGenerator.SeedData.xml"));

            var parms = new ParameterParser<ConsoleParameters>().Parse(args);

            if (parms.Help)
                new ParameterParser<ConsoleParameters>().WriteHelp(Console.Out);
            else
            {
                var wtp = new WaitThreadPool(Int32.Parse(parms.Concurrency));

                for (int i = 0; i < Int32.Parse(parms.PopulationSize); i++)
                    wtp.QueueUserWorkItem(RegisterPatient, parms);

                wtp.WaitOne();
            }
        }

        /// <summary>
        /// Creates the specified REST client 
        /// </summary>
        private static IRestClient CreateClient(String baseUri, bool secured)
        {

            return new RestClient(new SanteDB.DisconnectedClient.Core.Configuration.ServiceClientDescription()
            {
                Binding = new SanteDB.DisconnectedClient.Core.Configuration.ServiceClientBinding()
                {
                    ContentTypeMapper = new DefaultContentTypeMapper(),
                    Security = secured ? new SanteDB.DisconnectedClient.Core.Configuration.ServiceClientSecurity()
                    {
                        CredentialProvider = new TokenCredentialProvider(),
                        Mode = SanteDB.Core.Http.Description.SecurityScheme.Bearer,
                        PreemptiveAuthentication = true
                    } : null
                },
                Endpoint = new List<SanteDB.DisconnectedClient.Core.Configuration.ServiceClientEndpoint>()
                {
                    new SanteDB.DisconnectedClient.Core.Configuration.ServiceClientEndpoint()
                    {
                        Address = baseUri
                    }
                },
                Name = "default"
            });
        }

        /// <summary>
        /// Authenticates this system against the specified realm
        /// </summary>
        public static IPrincipal Authenticate(String realm, String user, String password)
        {

            var oauthRequest = new OAuthTokenRequest(user, password, "*")
            {
                ClientId = "fiddler",
                ClientSecret = "fiddler"
            };

            // Client for authentication
            try
            {
                using (var client = CreateClient($"{realm}/auth", false))
                {
                    client.Accept = "application/json";
                    var response = client.Post<OAuthTokenRequest, OAuthTokenResponse>("oauth2_token", "application/x-www-form-urlencoded", oauthRequest);
                    if (!String.IsNullOrEmpty(response.AccessToken))
                        AuthenticationContext.Current = new AuthenticationContext(new TokenClaimsPrincipal(response.AccessToken, response.IdToken, response.TokenType, response.RefreshToken));
                    else throw new Exception("Could not retrieve token from server");
                    return AuthenticationContext.Current.Principal;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not authenticate: {0}", e);
                throw new Exception($"Could not authenticate", e);
            }

        }

        /// <summary>
        /// Generate patients 
        /// </summary>
        public static void RegisterPatient(object state)
        {

            var parms = state as ConsoleParameters;

            // Authenticate
            if (!AuthenticationContext.Current.Principal.Identity.IsAuthenticated)
                Authenticate(parms.Realm, parms.UserName, parms.Password);

            // TODO: Send
            try
            {
                using (var client = CreateClient($"{parms.Realm}/hdsi", true))
                {

                    // Authority key?
                    if (!s_authorityKey.HasValue)
                        s_authorityKey = client.Get<Bundle>("AssigningAuthority", new KeyValuePair<string, object>("domainName", parms.IdentityDomain)).Item.First().Key;

                    // Generate the patient
                    var gender = s_random.Next() % 2 == 0 ? Guid.Parse("f4e3a6bb-612e-46b2-9f77-ff844d971198") : Guid.Parse("094941e9-a3db-48b5-862c-bc289bd7f86c");
                    var name = GenerateName(gender.ToString());
                    var p = new Patient()
                    {
                        DateOfBirth = DateTime.Now.Subtract(new TimeSpan(s_random.Next(Int32.Parse(parms.MaxAge)) * 365, 0, 0, 0)),
                        Addresses = new List<SanteDB.Core.Model.Entities.EntityAddress>() { GenerateAddress() },
                        DateOfBirthPrecision = SanteDB.Core.Model.DataTypes.DatePrecision.Day,
                        GenderConceptKey = gender,
                        Identifiers = new List<SanteDB.Core.Model.DataTypes.EntityIdentifier>()
                        {
                            new SanteDB.Core.Model.DataTypes.EntityIdentifier(s_authorityKey.Value, Guid.NewGuid().ToString().Substring(0, 8)),
                        },
                        LanguageCommunication = new List<SanteDB.Core.Model.Entities.PersonLanguageCommunication>()
                        {
                            new SanteDB.Core.Model.Entities.PersonLanguageCommunication("en", true)
                        },
                        Telecoms = new List<SanteDB.Core.Model.Entities.EntityTelecomAddress>() {
                            GenerateEmail(name),
                            GenerateTelephone()
                        },
                        Names = new List<SanteDB.Core.Model.Entities.EntityName>()
                        {
                            name
                        }
                    };

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var result = client.Post<Patient, Patient>("Patient", "application/xml", p);
                    sw.Stop();
                    Console.WriteLine("Registered {0} in {1} ms", result, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending patient: {0}", e);
                throw new Exception("Error sending patient", e);
            }

        }

        /// <summary>
        /// Generate a fake telephone number
        /// </summary>
        private static EntityTelecomAddress GenerateTelephone() => new EntityTelecomAddress(TelecomAddressUseKeys.WorkPlace, String.Format("030-304-{0:0000}", s_random.Next(9999)));

        /// <summary>
        /// Generate e-mail address
        /// </summary>
        private static EntityTelecomAddress GenerateEmail(EntityName name)
        {
            return new EntityTelecomAddress(TelecomAddressUseKeys.Public, $"{name.Component.First().Value}.{name.Component.Last().Value}@test.com");
        }

        /// <summary>
        /// Generate a fake address
        /// </summary>
        private static EntityAddress GenerateAddress()
        {
            return new EntityAddress(AddressUseKeys.HomeAddress, s_seedData.Streets.Get(s_random.Next()), s_seedData.Cities.Get(s_random.Next()), s_seedData.States.Get(s_random.Next()), "CA", String.Format("{0:00000}", s_random.Next(99999)));
        }

        /// <summary>
        /// Generate a name
        /// </summary>
        private static EntityName GenerateName(String genderConceptKey)
        {
            return new EntityName(NameUseKeys.OfficialRecord, s_seedData.FamilyNames.Get(s_random.Next()), s_seedData.GivenNames.Where(o => o.Gender == genderConceptKey).Get(s_random.Next()), s_seedData.GivenNames.Where(o => o.Gender == genderConceptKey).Get(s_random.Next()));
        }

    }
}

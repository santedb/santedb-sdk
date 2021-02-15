using MohawkCollege.Util.Console.Parameters;
using SanteDB.Core.Http;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Threading;
using SanteDB.DisconnectedClient.Http;
using SanteDB.DisconnectedClient.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SanteDB.Core.Api.Security;

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

            var seed = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            s_random = new Random(seed);
            s_seedData = SeedData.Load(typeof(Program).Assembly.GetManifestResourceStream("FakeDataGenerator.SeedData.xml"));

            var parms = new ParameterParser<ConsoleParameters>().Parse(args);

            if (parms.Help)
                new ParameterParser<ConsoleParameters>().WriteHelp(Console.Out);
            else if(Int32.Parse(parms.Concurrency) > 1)
            {
                Console.WriteLine("Starting as controller");
                var processes = new Process[Int32.Parse(parms.Concurrency)];
                for(int i = 0; i < processes.Length; i++)
                {
                    var processStart = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
                    processStart.Arguments = $"--popsize={parms.PopulationSize} --concurrency=1 --maxage={parms.MaxAge} --realm={parms.Realm} --user={parms.UserName} --password={parms.Password} --auth={parms.IdentityDomain}";
                    processes[i] = new Process();
                    processes[i].StartInfo = processStart;
                    processes[i].Start();
                }

                bool canExit = true;
                do
                {
                    canExit = true;
                    foreach (var itm in processes)
                        canExit &= itm.HasExited;
                    Thread.Sleep(1000);
                } while (!canExit);
            }
            else
            {
                Console.WriteLine("Starting as worker bee");

                for (int i = 0; i < Int32.Parse(parms.PopulationSize); i++)
                    try
                    {
                        RegisterPatient(parms);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't register - {0}", e.Message);
                    }

                
            }
        }

        /// <summary>
        /// Creates the specified REST client 
        /// </summary>
        private static IRestClient CreateClient(String baseUri, bool secured)
        {

            return new RestClient(new SanteDB.DisconnectedClient.Configuration.ServiceClientDescription()
            {
                Binding = new SanteDB.DisconnectedClient.Configuration.ServiceClientBinding()
                {
                    ContentTypeMapper = new DefaultContentTypeMapper(),
                    Security = secured ? new SanteDB.DisconnectedClient.Configuration.ServiceClientSecurity()
                    {
                        CredentialProvider = new TokenCredentialProvider(),
                        Mode = SanteDB.Core.Http.Description.SecurityScheme.Bearer,
                        PreemptiveAuthentication = true
                    } : null
                },
                Endpoint = new List<SanteDB.DisconnectedClient.Configuration.ServiceClientEndpoint>()
                {
                    new SanteDB.DisconnectedClient.Configuration.ServiceClientEndpoint()
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
                        AuthenticationContext.Current = new AuthenticationContext(new TokenClaimsPrincipal(response.AccessToken, response.IdToken, response.TokenType, response.RefreshToken, null));
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
            if (!AuthenticationContext.Current.Principal.Identity.IsAuthenticated ||
                AuthenticationContext.Current.Principal.Identity.Name == "ANONYMOUS" ||
                DateTime.Now.Minute % 5 == 0)
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
                            new SanteDB.Core.Model.DataTypes.EntityIdentifier(s_authorityKey.Value, GenerateCheckedIdentifier()),
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

        private static Random _generator = new Random();

        /// <summary>
        /// Genereate a checked identifier
        /// </summary>
        /// <returns></returns>
        private static string GenerateCheckedIdentifier()
        {
            // generate a random 10 digit number
            byte[] buffer = new byte[8];
            _generator.NextBytes(buffer);
            var source = BitConverter.ToUInt64(buffer, 0).ToString("0000000000").Substring(0, 10);

            // translate source into check digit seed
            string key = "0123456789";
            int seed = source
                .Select(t => key.IndexOf(t))
                .Aggregate(0, (current, index) => ((current + index) * 10) % 97);
            seed = (seed * 10) % 97;

            // build check digit
            int checkDigit = (97 - seed + 1) % 97;

            // append check digit (with 0 fill, needs to be 2 digits)
            string number = $"{source}{checkDigit:D2}";
            return number;

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

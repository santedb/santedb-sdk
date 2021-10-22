using MohawkCollege.Util.Console.Parameters;
using SanteDB.Core.Http;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.DisconnectedClient.Http;
using SanteDB.DisconnectedClient.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PatientImporter
{
	internal class Program
    {
        private static Guid enterpriseDomain = Guid.Empty;
        private static Guid mrnDomain = Guid.Empty;
        private static Guid ssnDomain = Guid.Empty;
        private static Guid febrlDomain = Guid.Empty;


        private static async Task Main(string[] args)
        {
            var parms = new ParameterParser<ConsoleParameters>().Parse(args);

            Console.WriteLine("SanteDB PatientImporter v{0} ({1})", Assembly.GetEntryAssembly().GetName().Version, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Console.WriteLine("Copyright (C) 2015-2019 See NOTICE for contributors");

            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Console.WriteLine("Could not complete operation - {0}", e.ExceptionObject);
            };

            if (parms.Help)
                new ParameterParser<ConsoleParameters>().WriteHelp(Console.Out);
            else
            {
                // Authenticate
                if (!AuthenticationContext.Current.Principal.Identity.IsAuthenticated ||
                    AuthenticationContext.Current.Principal.Identity.Name == "ANONYMOUS")
                    Authenticate(parms.Realm, parms.UserName, parms.Password);

                using (var client = CreateClient($"{parms.Realm}/hdsi", true))
                {
                    // Authority key?
                    if (!string.IsNullOrEmpty(parms.EnterpriseIdDomain))
                        enterpriseDomain = client.Get<Bundle>("AssigningAuthority", new KeyValuePair<string, object>("domainName", parms.EnterpriseIdDomain)).Item.First().Key.Value;
                    if (!string.IsNullOrEmpty(parms.MrnDomain))
                        mrnDomain = client.Get<Bundle>("AssigningAuthority", new KeyValuePair<string, object>("domainName", parms.MrnDomain)).Item.First().Key.Value;
                    if (!string.IsNullOrEmpty(parms.SsnDomain))
                        ssnDomain = client.Get<Bundle>("AssigningAuthority", new KeyValuePair<string, object>("domainName", parms.SsnDomain)).Item.First().Key.Value;
                    if (!string.IsNullOrEmpty(parms.FebrlDomain))
	                    febrlDomain = client.Get<Bundle>("AssigningAuthority", new KeyValuePair<string, object>("domainName", parms.FebrlDomain)).Item.First().Key.Value;
                }

                // Process files
                var files = parms.Source.OfType<String>().SelectMany(s =>
                {
                    if (s.Contains("*"))
                        return Directory.GetFiles(Path.GetDirectoryName(s), Path.GetFileName(s));
                    else
                        return new String[] { s };
                });

                switch (parms.DatasetName.ToLowerInvariant())
                {
                    case "onc":
	                    foreach (var f in files)
	                    {
		                    await SeedOncDatasetAsync(new { FileName = f, Parameters = parms });
	                    }
                        break;
                    case "febrl":
	                    foreach (var f in files)
	                    {
		                    await SeedFebrlDatasetAsync(new { FileName = f, Parameters = parms });
	                    }
                        break;
                }
                
            }
        }

        /// <summary>
        /// Creates the specified REST client
        /// </summary>
        private static IRestClient CreateClient(String baseUri, bool secured)
        {
            return new RestClient(new SanteDB.DisconnectedClient.Configuration.ServiceClientDescriptionConfiguration()
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
                    var response = client.Post<OAuthTokenRequest, OAuthTokenResponse>("oauth2_token", "application/x-www-form-urlencoded", oauthRequest);
                    if (!String.IsNullOrEmpty(response.AccessToken))
                        AuthenticationContext.EnterContext(new TokenClaimsPrincipal(response.AccessToken, response.IdToken, response.TokenType, response.RefreshToken, null));
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
        /// Process / import the specified FEBRL dataset asynchronously.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Task.</returns>
        private static Task SeedFebrlDatasetAsync(object state)
        {
            var parameters = state as dynamic;
            Console.WriteLine("Start Processing of {0}...", parameters.FileName);
            var settings = parameters.Parameters as ConsoleParameters;

            try
            {
	            if (febrlDomain == Guid.Empty)
	            {
		            throw new InvalidOperationException("Unable to locate assigning authority for the FEBRL domain. Please specify an NSID value");
	            }

	            if (ssnDomain == Guid.Empty)
	            {
		            throw new InvalidOperationException("Unable to locate assigning authority for the SSN domain, the SSN domain is required when seeding FEBRL data. Please specify an NSID value");
	            }

                var counter = 1;

	            using (var client = CreateClient($"{parameters.Parameters.Realm}/hdsi", true))
                {
                    using (StreamReader tw = File.OpenText(parameters.FileName))
                    {
                        tw.ReadLine();
                        while (!tw.EndOfStream)
                        {
                            try
                            {
                                string[] data = tw.ReadLine().Split(',');

                                // Authenticate
                                Authenticate(parameters.Parameters.Realm, parameters.Parameters.UserName, parameters.Parameters.Password);

                                var patient = new Patient
                                {
	                                Names = new List<EntityName>
	                                {
		                                new EntityName(NameUseKeys.OfficialRecord, data[2], data[1])
	                                }.ToList(),
	                                DateOfBirth = string.IsNullOrEmpty(data[9]) ? null : (DateTime?)DateTime.ParseExact(data[9], "yyyyMMdd", CultureInfo.InvariantCulture)
                                };

                                if (patient.DateOfBirth != null)
                                {
                                    patient.DateOfBirthPrecision = DatePrecision.Day;
                                }

                                var streetAddress = string.IsNullOrEmpty(data[3]) ? data[4] : data[3] + " " + data[4];
                                var address = new EntityAddress(AddressUseKeys.HomeAddress, streetAddress, data[6], data[8], "US", data[7]);

                                if (!string.IsNullOrEmpty(data[5]))
                                {
	                                address.Component.Add(new EntityAddressComponent(AddressComponentKeys.StreetAddressLine, data[5]));
                                }

                                patient.Addresses = new List<EntityAddress>
                                {
                                    address
                                };

                                patient.Identifiers = new List<EntityIdentifier>
                                { 
	                                new EntityIdentifier(ssnDomain, data[10]), 
	                                new EntityIdentifier(febrlDomain, data[0])
                                }.Where(o => !string.IsNullOrEmpty(o.Value) && Guid.Empty != o.AuthorityKey.Value).ToList();

                                
								var sw = new Stopwatch();

                                sw.Start();

								var result = client.Post<Patient, Patient>("Patient", "application/xml", patient);

								sw.Stop();

                                //Console.WriteLine("Registered {0} in {1} ms", result, sw.ElapsedMilliseconds);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("WRN: Couldn't process {0} - {1}", parameters.FileName, e);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERR: Couldn't process {0} - {1}", parameters.FileName, e);
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Process / import the specified ONC dataset asynchronously.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Task.</returns>
        private static Task SeedOncDatasetAsync(object state)
        {
            var parameters = state as dynamic;
            Console.WriteLine("Start Processing of {0}...", parameters.FileName);
            var settings = parameters.Parameters as ConsoleParameters;

            try
            {
                using (var client = CreateClient($"{parameters.Parameters.Realm}/hdsi", true))
                {
                    using (var tw = File.OpenText(parameters.FileName))
                    {
                        tw.ReadLine();
                        while (!tw.EndOfStream)
                        {
                            try
                            {
                                var data = tw.ReadLine().Split(',');

                                // Authenticate
                                Authenticate(parameters.Parameters.Realm, parameters.Parameters.UserName, parameters.Parameters.Password);

                                var aliasParts = new String[0];
                                if (data[18].Contains(" "))
                                    aliasParts = data[18].Split(' ');

                                var patient = new Patient()
                                {
                                    Names = new List<SanteDB.Core.Model.Entities.EntityName>()
                                {
                                    new SanteDB.Core.Model.Entities.EntityName(NameUseKeys.OfficialRecord, data[1], data[2], data[3], data[4]),
                                    aliasParts.Length == 0 ? null :
                                        aliasParts.Length > 1 ? new EntityName(NameUseKeys.Pseudonym, aliasParts.Last(), aliasParts.Take(aliasParts.Length - 1).ToArray())
                                        : new EntityName(NameUseKeys.Pseudonym, aliasParts.First())
                                }.OfType<EntityName>().ToList(),
                                    DateOfBirth = String.IsNullOrEmpty(data[5]) ? null : new DateTime(1970, 01, 01).AddSeconds(Int32.Parse(data[5]) * 10000),
                                    DateOfBirthPrecision = SanteDB.Core.Model.DataTypes.DatePrecision.Year,
                                    GenderConceptKey = data[6] == "FEMALE" ? Guid.Parse("f4e3a6bb-612e-46b2-9f77-ff844d971198") : Guid.Parse("094941e9-a3db-48b5-862c-bc289bd7f86c"),
                                    Addresses = new List<SanteDB.Core.Model.Entities.EntityAddress>()
                                {
                                    new SanteDB.Core.Model.Entities.EntityAddress(AddressUseKeys.HomeAddress, data[8], data[13], data[14], "US", data[10])
                                },
                                    Identifiers = new EntityIdentifier[]
                                    {
                                    new EntityIdentifier(enterpriseDomain, data[0]),
                                    new EntityIdentifier(ssnDomain, data[7]),
                                    new EntityIdentifier(mrnDomain, data[12])
                                    }.Where(o => !String.IsNullOrEmpty(o.Value) && Guid.Empty != o.AuthorityKey.Value).ToList(),
                                    Relationships = new List<SanteDB.Core.Model.Entities.EntityRelationship>()
                                {
                                    String.IsNullOrEmpty(data[11]) ? null : new SanteDB.Core.Model.Entities.EntityRelationship(EntityRelationshipTypeKeys.Mother, new Person() {
                                        Names = new List<EntityName>()
                                        {
                                            new EntityName(NameUseKeys.MaidenName, data[11], "", "")
                                        }
                                    })
                                }.OfType<EntityRelationship>().ToList(),
                                    Telecoms = new EntityTelecomAddress[]
                                    {
                                    new EntityTelecomAddress(TelecomAddressUseKeys.Public, data[15]) { TypeConceptKey = TelecomAddressTypeKeys.Telephone },
                                    new EntityTelecomAddress(TelecomAddressUseKeys.Public, data[16]) { TypeConceptKey = TelecomAddressTypeKeys.Telephone },
                                    new EntityTelecomAddress(TelecomAddressUseKeys.Public, data[17]) { TypeConceptKey = TelecomAddressTypeKeys.Internet },
                                    }.Where(o => !string.IsNullOrEmpty(o.Value)).ToList()
                                };

                                if (patient.Relationships.Count > 0)
                                {
                                    var entity = client.Post<Entity, Entity>("Entity", "application/xml", patient.Relationships.First().TargetEntity);
                                    patient.Relationships[0].TargetEntityKey = entity.Key;
                                }

                                Stopwatch sw = new Stopwatch();
                                sw.Start();
                                var result = client.Post<Patient, Patient>("Patient", "application/xml", patient);
                                sw.Stop();
                                Console.WriteLine("Registered {0} in {1} ms", result, sw.ElapsedMilliseconds);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("WRN: Couldn't process {0} - {1}", parameters.FileName, e);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERR: Couldn't process {0} - {1}", parameters.FileName, e);
            }

            return Task.CompletedTask;
        }
    }
}
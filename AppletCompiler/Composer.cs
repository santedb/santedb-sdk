using PakMan.Repository;
using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PakMan
{
    /// <summary>
    /// A tool that composes data
    /// </summary>
    public class Composer
    {

        private PakManParameters m_parms;

        /// <summary>
        /// Creates a new composer
        /// </summary>
        /// <param name="parms"></param>
        public Composer(PakManParameters parms)
        {
            this.m_parms = parms;
        }

        /// <summary>
        /// Compose multiple PAK files into a solution
        /// </summary>
        public int Compose()
        {
            try
            {
                AppletManifest mfst = null;
                using (FileStream fs = File.OpenRead(this.m_parms.Source))
                    mfst = AppletManifest.Load(fs);

                var slnPak = mfst.CreatePackage();
                
                AppletSolution sln = new AppletSolution();
                sln.Meta = slnPak.Meta;
                sln.PublicKey = slnPak.PublicKey;
                sln.Manifest = slnPak.Manifest;

                sln.Include = new List<AppletPackage>();
                
                foreach (var pfile in sln.Meta.Dependencies.ToArray())
                {
                    AppletPackage pkg = PackageRepositoryUtil.GetFromAny(pfile.Id, new Version(pfile.Version));

                    if (pkg == null)
                        throw new KeyNotFoundException($"Package {pfile.Id} {pfile.Version} not found");
                    else
                    {
                        
                        Emit.Message("INFO","Including {0} version {1}..", pfile.Id, pfile.Version);
                        sln.Meta.Dependencies.RemoveAll(o => o.Id == pkg.Meta.Id);

                        if (!String.IsNullOrEmpty(this.m_parms.SignKey) && pkg.Meta.Signature == null)
                        {
                            Emit.Message("WARN","Package {0} is not signed, but you're signing your package. We'll sign it using your key", pkg.Meta.Id);
                            pkg = new Signer(this.m_parms).CreateSignedPackage(pkg.Unpack());
                        }
                        sln.Include.Add(pkg);
                    }
                }

                sln.Meta.Hash = SHA256.Create().ComputeHash(sln.Include.SelectMany(o=>o.Manifest).ToArray());
                // Sign the signature package
                if (!String.IsNullOrEmpty(this.m_parms.SignKey))
                    new Signer(this.m_parms).CreateSignedSolution(sln);

                // Now save
                using (FileStream fs = File.Create(this.m_parms.Output ?? Path.ChangeExtension(sln.Meta.Id, ".sln.pak")))
                    sln.Save(fs);

                return 0;
            }
            catch (System.Exception e)
            {
                Emit.Message("ERROR", e.Message);
                //Console.Error.WriteLine("Cannot compose solution {0}: {1}", this.m_parms.Source, e);
                return -1;
            }
        }

    }
}

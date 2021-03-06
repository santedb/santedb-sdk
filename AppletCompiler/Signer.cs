﻿using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PakMan
{
    /// <summary>
    /// Tool for signing packages and data
    /// </summary>
    internal class Signer
    {

        // Parameters
        private PakManParameters m_parms;

        /// <summary>
        /// Create a new signer
        /// </summary>
        /// <param name="parameters"></param>
        public Signer(PakManParameters parameters)
        {
            this.m_parms = parameters;
        }

        /// <summary>
        /// Sign an existing package
        /// </summary>
        public int Sign()
        {
            try
            {
                AppletPackage pkg = null;
                using (FileStream fs = File.OpenRead(this.m_parms.Source))
                    pkg = AppletPackage.Load(fs);

                Emit.Message("INFO","Will sign package {0}", pkg.Meta);
                pkg = this.CreateSignedPackage(pkg.Unpack());
                using (FileStream fs = File.Create(this.m_parms.Output ?? Path.ChangeExtension(this.m_parms.Source, ".signed.pak")))
                    pkg.Save(fs);
                return 0;
            }
            catch (Exception e)
            {
                Emit.Message("ERROR", "Cannot sign package: {0}", e);
                return -0232;
            }
        }

        /// <summary>
        /// Create a signed package
        /// </summary>
        public AppletPackage CreateSignedPackage(AppletManifest mfst)
        {
            try
            {
                if (String.IsNullOrEmpty(this.m_parms.SignPassword))
                {
                    using (var frmKey = new frmKeyPassword(this.m_parms.SignKey))
                        if (frmKey.ShowDialog() == DialogResult.OK)
                            this.m_parms.SignPassword = frmKey.Password;
                }
                else if (File.Exists(this.m_parms.SignPassword))
                    this.m_parms.SignPassword = File.ReadAllText(this.m_parms.SignPassword);

                X509Certificate2 signCert = new X509Certificate2(this.m_parms.SignKey, this.m_parms.SignPassword);

                mfst.Info.TimeStamp = DateTime.Now; // timestamp
                mfst.Info.PublicKeyToken = signCert.Thumbprint;
                var retVal = mfst.CreatePackage();

                retVal.Meta.Hash = SHA256.Create().ComputeHash(retVal.Manifest);
                retVal.Meta.PublicKeyToken = signCert.Thumbprint;

                if (this.m_parms.EmbedCertificate)
                    retVal.PublicKey = signCert.Export(X509ContentType.Cert);

                if (!signCert.HasPrivateKey)
                    throw new SecurityException($"Provided key {this.m_parms.SignKey} has no private key");
                RSACryptoServiceProvider rsa = signCert.PrivateKey as RSACryptoServiceProvider;
                retVal.Meta.Signature = rsa.SignData(retVal.Manifest, CryptoConfig.MapNameToOID("SHA1"));
                return retVal;
            }
            catch (Exception e)
            {
                Emit.Message("ERROR", "Error signing package: {0}", e);
                return null;
            }
        }

        /// <summary>
        /// Create a signed package
        /// </summary>
        public AppletPackage CreateSignedSolution(AppletSolution sln)
        {
            try
            {
                if (String.IsNullOrEmpty(this.m_parms.SignPassword))
                {
                    using (var frmKey = new frmKeyPassword(this.m_parms.SignKey))
                        if (frmKey.ShowDialog() == DialogResult.OK)
                            this.m_parms.SignPassword = frmKey.Password;
                }
                else if (File.Exists(this.m_parms.SignPassword))
                    this.m_parms.SignPassword = File.ReadAllText(this.m_parms.SignPassword);

                X509Certificate2 signCert = new X509Certificate2(this.m_parms.SignKey, this.m_parms.SignPassword);

                // Combine all the manifests
                sln.Meta.Hash = SHA256.Create().ComputeHash(sln.Include.SelectMany(o=>o.Manifest).ToArray());
                sln.Meta.PublicKeyToken = signCert.Thumbprint;

                if (this.m_parms.EmbedCertificate)
                    sln.PublicKey = signCert.Export(X509ContentType.Cert);

                if (!signCert.HasPrivateKey)
                    throw new SecurityException($"Provided key {this.m_parms.SignKey} has no private key");
                RSACryptoServiceProvider rsa = signCert.PrivateKey as RSACryptoServiceProvider;
                sln.Meta.Signature = rsa.SignData(sln.Include.SelectMany(o => o.Manifest).ToArray(), CryptoConfig.MapNameToOID("SHA1"));
                return sln;
            }
            catch (Exception e)
            {
                Emit.Message("ERROR", "Error signing package: {0}", e);
                return null;
            }
        }

    }
}

/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justin
 * DatERROR: 2018-6-27
 */
using SanteDB.Core.Applets;
using SanteDB.Core.Applets.Model;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers.Tar;
using SharpCompress.Readers.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace PakMan
{
    /// <summary>
    /// Distribution composer
    /// </summary>
    public class Distributor
    {
        /// <summary>
        /// Parameters
        /// </summary>
        private PakManParameters m_parameters;

        // The package
        private AppletPackage m_package;

        /// <summary>
        /// Parameters for the distributor
        /// </summary>
        public Distributor(PakManParameters parameters)
        {
            this.m_parameters = parameters;
        }

        /// <summary>
        /// Package the assets
        /// </summary>
        /// <returns></returns>
        public int Package()
        {

            using (FileStream fs = File.OpenRead(this.m_parameters.Output))
                this.m_package = AppletSolution.Load(fs);

            // Package the android APK project
            if(this.m_parameters.DcdrAssets.Contains("android"))
                this.PackageApk();

            // Package the DCG project
            if(this.m_parameters.DcdrAssets.Contains("gateway"))
                this.PackageDcg();

            return 1;
        }


        /// <summary>
        /// Package a DCG distribution
        /// </summary>
        private void PackageDcg()
        {
        }

        /// <summary>
        /// Package an APK to the output directory
        /// </summary>
        private void PackageApk()
        {
            Emit.Message("INFO","Will package {0}.apk ...", this.m_package.Meta.Id);

            var workingDir = Path.Combine(Path.GetTempPath(), "dcg-apk", this.m_package.Meta.Id);
            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);

            this.ExtractTarget("android.zip", workingDir);

            var appletCollection = new AppletCollection();

            // Next slip in the applets from our output
            Emit.Message("INFO", "Slipstreaming Applets");
            if (this.m_package is AppletSolution solution)
            {
                foreach (var pkg in solution.Include)
                {
                    this.SerializeApplet(pkg, Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Assets", "Applets"), Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "SanteDB.DisconnectedClient.Android.csproj"));
                    appletCollection.Add(pkg.Unpack());
                }
            }
            else
            {
                this.SerializeApplet(this.m_package, Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Assets", "Applets"), Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "SanteDB.DisconnectedClient.Android.csproj"));
                appletCollection.Add(this.m_package.Unpack());
            }

            // Next setup the android manifest
            var manifest = new XmlDocument();
            manifest.Load(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Properties", "AndroidManifest.xml"));
            manifest.DocumentElement.SetAttribute("versionCode", "http://schemas.android.com/apk/res/android", new Version(this.m_package.Meta.Version).Major.ToString());
            manifest.DocumentElement.SetAttribute("versionName", "http://schemas.android.com/apk/res/android", this.m_package.Meta.Version);
            manifest.DocumentElement.SetAttribute("package", this.m_package.Meta.Id);
            manifest.Save(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Properties", "AndroidManifest.xml"));

            // Get the icon
            var iconAsset = appletCollection.ResolveAsset(this.m_package.Meta.Icon);
            if(iconAsset?.MimeType == "image/png")
            {
                var imageData = appletCollection.RenderAssetContent(iconAsset);
                Emit.Message("INFO","Slipstream Icons...");
                File.WriteAllBytes(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources", "drawable", "icon.png"), imageData);
                File.WriteAllBytes(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources", "drawable", "logo.png"), imageData);
                File.WriteAllBytes(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources", "drawable", "logo_lg.png"), imageData);

                foreach (var dir in Directory.GetDirectories(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources"), "mipmap-*"))
                    File.WriteAllBytes(Path.Combine(dir, "Icon.png") ,imageData);
            }

            // Swap out the translation
            var strings = new XmlDocument();
            strings.Load(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources", "values", "Strings.xml"));
            strings.SelectSingleNode("//*[local-name() = 'string'][@name = 'app_name']").InnerText = this.m_package.Meta.GetName("en", true);
            strings.Save(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android", "Resources", "values", "Strings.xml"));

            // Locate MSBUILD Path
            if(String.IsNullOrEmpty(this.m_parameters.MsBuild))
            {
                var vsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio");
                if (Directory.Exists(vsDir))
                    this.m_parameters.MsBuild = Directory.GetFiles(vsDir, "msbuild.exe", SearchOption.AllDirectories).LastOrDefault();
            }

            if(!File.Exists(this.m_parameters.MsBuild))
            {
                Emit.Message("ERROR", "Cannot find Visual Studio MSBUILD Tooling");
                throw new InvalidOperationException("Missing Visual Studio Build Tooling");
            }

            var arguments = new String[] {
                $"/p:Configuration=Pakman /m:1 /t:restore \"{Path.Combine(workingDir, "santedb-dc-android.sln")}\"",
                $"/p:Configuration=Pakman /m:1 /t:SignAndroidPackage \"{Path.Combine(workingDir, "santedb-dc-android.sln")}\""
            };


            foreach (var args in arguments)
            {
                var processInfo = new ProcessStartInfo(this.m_parameters.MsBuild);
                processInfo.Arguments = args;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;
                processInfo.UseShellExecute = false;
                Emit.Message("INFO", "Running {0} {1}", processInfo.FileName, processInfo.Arguments);

                using (var process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.EnableRaisingEvents = true;

                    var mre = new ManualResetEventSlim(false);
                    process.ErrorDataReceived += (o, e) => Console.WriteLine(e.Data);
                    process.OutputDataReceived += (o, e) => Console.WriteLine(e.Data);
                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    process.WaitForExit();
                    process.Close();
                }
            }

            // Copy the output of the APK
            foreach(var apk in Directory.GetFiles(Path.Combine(workingDir, "SanteDB.DisconnectedClient.Android","bin","Pakman"), "*.apk"))
            {
                var dest = Path.Combine(Path.GetDirectoryName(this.m_parameters.Output), Path.GetFileName(apk));
                Emit.Message("INFO", "Copying {0} to {1}...", apk, dest);
                File.Copy(apk, dest, true);
            }

        }

        /// <summary>
        /// Serialize the applet to a file
        /// </summary>
        private void SerializeApplet(AppletPackage pkg, string target, String csproj = null)
        {

            Emit.Message("INFO", "Slipsteam {0} -> {1}", pkg.Meta.Id, target);

            // Add the file
            using (var fs = File.Create(Path.Combine(target, pkg.Meta.Id) + ".pak"))
                pkg.Save(fs);

            // Save the CSPROJ info
            if (!String.IsNullOrEmpty(csproj)) {
                XmlDocument doc = new XmlDocument();
                doc.Load(csproj);
                // Get Android Assets - Do they exist for this object?
                var assetPath = Path.Combine(target.Replace(Path.GetDirectoryName(csproj), "").Substring(1) , pkg.Meta.Id + ".pak");

                var node = doc.SelectSingleNode($"//*[local-name() = 'AndroidAsset'][@Include = '{assetPath}']");
                if (node == null) {
                    var itemElement = doc.CreateElement("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003")
                        .AppendChild(doc.CreateElement("AndroidAsset", "http://schemas.microsoft.com/developer/msbuild/2003"))
                        .Attributes.Append(doc.CreateAttribute("Include"));
                    itemElement.Value = assetPath;
                    doc.DocumentElement.AppendChild(itemElement.OwnerElement.ParentNode);
                    doc.Save(csproj);
                }
               

            }
        }

        /// <summary>
        /// Extract the target 
        /// </summary>
        private void ExtractTarget(string fileName, string output)
        {
            var qualifiedFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "distribution", fileName);
            if (!File.Exists(qualifiedFile))
                throw new FileNotFoundException(qualifiedFile);
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            using (var fs = File.OpenRead(qualifiedFile))
            using (var tar = ZipReader.Open(fs))
                while (tar.MoveToNextEntry())
                {

                    string outName = Path.Combine(output, tar.Entry.Key);
                    if (!Directory.Exists(Path.GetDirectoryName(outName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(outName));
                    Emit.Message("INFO"," {0} > {1}", tar.Entry.Key, outName);

                    if (!tar.Entry.IsDirectory)
                        using (var ofs = File.Create(outName))
                            tar.WriteEntryTo(ofs);
                }
        }
    }
}
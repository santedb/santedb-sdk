using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities.Configuration;
using SanteDB.Core.Applets.Model;

namespace PakMan.Packers
{
    public class JavaScriptPacker : IFilePacker
    {
        /// <summary>
        /// Extensions to be packaged
        /// </summary>
        public string[] Extensions => new String[] { ".js" };

        /// <summary>
        /// Process the file
        /// </summary>
        public AppletAsset Process(string file, bool optimize)
        {
            try
            {
                String content = File.ReadAllText(file);
                if (optimize && !file.Contains("rules") && !file.Contains(".min.js"))
                {
                    var minifier = new Microsoft.Ajax.Utilities.Minifier();

                    var settings = new Microsoft.Ajax.Utilities.CodeSettings()
                    {
                        MinifyCode = true,
                        RemoveUnneededCode = false,
                        RemoveFunctionExpressionNames = false,
                        StripDebugStatements = true,
                        LocalRenaming = Microsoft.Ajax.Utilities.LocalRenaming.KeepAll,
                        PreserveFunctionNames = true,
                        IgnoreAllErrors = true,
                        ConstStatementsMozilla = true,
                        MacSafariQuirks = true,
                        TermSemicolons = false,
                        AmdSupport = true,
                        KnownGlobalNamesList = "async,await"
                    };

                    // HACK: Doesn't like async/await
                    var result = minifier.MinifyJavaScript(content.Replace("await ", "await=").Replace("async ", "async="), settings);
                    content = result.Replace("await=", "await ").Replace("async=", "async ");

                    foreach (var i in minifier.ErrorList)
                        Console.WriteLine("W: JavaScript Error {0} @ {1}:{2}", i.Message, i.StartLine, i.StartColumn);
                    
                }
                return new AppletAsset()
                {
                    MimeType = "text/javascript",
                    Content = PakManTool.CompressContent(content)
                };

            }
            catch (Exception e)
            {
                Console.Write("E: Cannot process JavaScript file {0} : {1}", file, e.Message);
                throw;
            }
        }
    }
}

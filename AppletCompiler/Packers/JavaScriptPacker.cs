using SanteDB.Core.Applets.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;

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
                    var minifier = new Ext.Net.Utilities.JSMin();
                    // HACK : JSMIN Hates /// Reference 
                    content = new Regex(@"\/\/\/\s?\<Reference.*", RegexOptions.IgnoreCase).Replace(content, "");
                    content = minifier.Minify(content);
                }
                return new AppletAsset()
                {
                    MimeType = "text/javascript",
                    Content = PakManTool.CompressContent(content)
                };

            }
            catch (Exception e)
            {
                Emit.Message("ERROR", "Cannot process JavaScript file {0} : {1}", file, e.Message);
                throw;
            }
        }
    }
}

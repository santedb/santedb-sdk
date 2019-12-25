using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Applets.Model;

namespace PakMan.Packers
{
    /// <summary>
    /// CSS file packager
    /// </summary>
    public class CssPacker : IFilePacker
    {
        /// <summary>
        /// Extensions for this CSS packager
        /// </summary>
        public string[] Extensions => new string[] { ".css" };

        /// <summary>
        /// Process this package
        /// </summary>
        public AppletAsset Process(string file, bool optimize)
        {
            try
            {
                if (optimize && !file.EndsWith(".min.css"))
                {
                    var minifier = new Microsoft.Ajax.Utilities.Minifier();

                    var content = minifier.MinifyStyleSheet(file, new Microsoft.Ajax.Utilities.CssSettings()
                    {
                        BlocksStartOnSameLine = Microsoft.Ajax.Utilities.BlockStart.SameLine, 
                        CommentMode = Microsoft.Ajax.Utilities.CssComment.None,
                        CssType = Microsoft.Ajax.Utilities.CssType.FullStyleSheet,
                        IgnoreAllErrors = true,
                        LineBreakThreshold = 240,
                        RemoveEmptyBlocks = true
                    });
                    return new AppletAsset()
                    {
                        MimeType = "text/css",
                        Content = PakManTool.CompressContent(content)
                    };
                }
                else
                    return new AppletAsset()
                    {
                        MimeType = "text/css",
                        Content = PakManTool.CompressContent(File.ReadAllText(file))
                    };
            }
            catch(Exception e)
            {
                Console.WriteLine("E: Error processing CSS file {0}: {1}", file, e.Message);
                throw;
            }
        }
    }
}

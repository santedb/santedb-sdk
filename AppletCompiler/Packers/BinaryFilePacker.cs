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
    /// Binary file package
    /// </summary>
    public class BinaryFilePacker : IFilePacker
    {
        /// <summary>
        /// Extnesions
        /// </summary>
        public string[] Extensions => new string[] { "*" };

        /// <summary>
        /// Process the file
        /// </summary>
        public AppletAsset Process(string file, bool optimize)
        {
            try
            {
                var mime = System.Web.MimeMapping.GetMimeMapping(file);
                if (String.IsNullOrEmpty(mime))
                    mime = "application/x-octet-stream";
                return new AppletAsset()
                {
                    MimeType = mime,
                    Content = PakManTool.CompressContent(File.ReadAllBytes(file))
                };
            }
            catch(Exception e)
            {
                Console.WriteLine("E: Cannot process {0}: {1}", file, e.Message);
                throw;
            }
        }
    }
}

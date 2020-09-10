using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SanteDB.Core.Applets.Model;

namespace PakMan.Packers
{
    public class JsonPacker : IFilePacker
    {
        /// <summary>
        /// Extensions to be packaged
        /// </summary>
        public string[] Extensions => new String[] { ".json" };

        /// <summary>
        /// Process the file
        /// </summary>
        public AppletAsset Process(string file, bool optimize)
        {
            try
            {
                String content = File.ReadAllText(file);
              
                return new AppletAsset()
                {
                    MimeType = "application/json",
                    Content = PakManTool.CompressContent(content)
                };

            }
            catch (Exception e)
            {
                Emit.Message("ERROR","Cannot process JavaScript file {0} : {1}", file, e.Message);
                throw;
            }
        }
    }
}

using PakMan.Packers;
using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PakMan
{
    /// <summary>
    /// Package manager constants
    /// </summary>
    internal static class PakManTool
    {

        // File packers
        private static IDictionary<String, IFilePacker> m_packers;

        /// <summary>
        /// XML namespace for applets
        /// </summary>
        public const string XS_APPLET = "http://santedb.org/applet";

        /// <summary>
        /// XML namepace for html
        /// </summary>
        public const string XS_HTML = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// Resolve the specified applet name
        /// </summary>
        internal static String TranslatePath(string value)
        {

            return value?.ToLower().Replace("\\", "/");
        }

        /// <summary>
        /// Gets the file packager for the specifie string
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static IFilePacker GetPacker(String file)
        {
            var ext = Path.GetExtension(file);
            if (m_packers == null) 
                m_packers = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.ExportedTypes)
                    .Where(t => typeof(IFilePacker).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .Select(t => Activator.CreateInstance(t))
                    .OfType<IFilePacker>()
                    .SelectMany(i => i.Extensions.Select(e => new { Ext = e, Pakr = i }))
                    .ToDictionary(o => o.Ext, o => o.Pakr);
            

            if (m_packers.TryGetValue(ext, out IFilePacker retVal))
                return retVal;
            else return m_packers["*"];
        }

        /// <summary>
        /// Apply version code
        /// </summary>
        internal static string ApplyVersion(string version)
        {
            return version.Replace("*", (((DateTime.Now.Subtract(new DateTime(DateTime.Now.Year, 1, 1)).Ticks >> 24) % 10000)).ToString("0000"));
        }

        /// <summary>
        /// Compress content
        /// </summary>
        internal static byte[] CompressContent(object content)
        {

            using (var ms = new MemoryStream())
            {
                using (var cs = new SharpCompress.Compressors.LZMA.LZipStream(ms, SharpCompress.Compressors.CompressionMode.Compress))
                {
                    byte[] data = content as byte[];
                    if (data == null)
                        data = System.Text.Encoding.UTF8.GetBytes(content.ToString());
                    cs.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SanteDB.Core.Applets.Model;

namespace PakMan.Packers
{
    /// <summary>
    /// Packager for XML
    /// </summary>
    public class XmlPacker : IFilePacker
    {
        /// <summary>
        /// Extensions
        /// </summary>
        public virtual string[] Extensions => new string[] { ".xml", ".dataset" };

        /// <summary>
        /// Process the XML file
        /// </summary>
        public virtual AppletAsset Process(string file, bool optimize)
        {

            // Verify that the file is XML
            try
            {
                var xe = new XmlDocument();
                xe.Load(file);

                if (optimize)
                    using (var ms = new MemoryStream())
                    using (var xw = XmlWriter.Create(ms, new XmlWriterSettings() { Indent = false, OmitXmlDeclaration = true }))
                    {
                        xe.WriteContentTo(xw);
                        xw.Flush();
                        return new AppletAsset()
                        {
                            MimeType = "text/xml",
                            Content = PakManTool.CompressContent(ms.ToArray())
                        };
                    }
                else
                    return new AppletAsset()
                    {
                        MimeType = "text/xml",
                        Content = PakManTool.CompressContent(File.ReadAllBytes(file))
                    };
            }
            catch (XmlException e)
            {
                Emit.Message("ERROR"," {0} is not well formed - {1} - @{2}:{3}", file, e.Message, e.LineNumber, e.LinePosition);

                throw;
            }
        }
    }
}

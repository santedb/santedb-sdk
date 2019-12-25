using SanteDB.Core.Applets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PakMan.Packers
{
    /// <summary>
    /// File packager
    /// </summary>
    internal interface IFilePacker
    {

        /// <summary>
        /// Extensions
        /// </summary>
        string[] Extensions { get; }

        /// <summary>
        /// Processes the specified file into the applet format
        /// </summary>
        AppletAsset Process(String file, bool optimize);

    }
}

using SanteDB.Core.Applets.Model;
using System;

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Utility
{
    public class FilePathUtility
    {
        public static string LocalAppDataFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "//JoshLmao//Repose//"; } }
        public static string PreferencesFilePath { get { return LocalAppDataFolder + "Prefs.json"; } }
    }
}

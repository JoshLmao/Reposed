﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Utility
{
    public class FilePathUtility
    {
        public static string LocalAppDataFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "//JoshLmao//Reposed//"; } }

        public static string PreferencesFilePath { get { return LocalAppDataFolder + "Prefs.json"; } }
        public static string BackupSettingsFilePath { get { return LocalAppDataFolder + "BackupSettings.json"; } }
    }
}

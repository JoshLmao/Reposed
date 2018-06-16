using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Utility
{
    public class GitHelper
    {
        public static string GetCloneCommand(string cloneUrl, string backupFolderPath)
        {
            bool dirExists = Directory.Exists(backupFolderPath);
            if (dirExists)
                return "remote update";

            string gitCommand = "clone --mirror";
            return $"{gitCommand} {cloneUrl} \"{backupFolderPath}\"";
        }

        public static string GetHTTPSCloneCommand(string cloneUrl, string backupFolderPath)
        {
            bool dirExists = Directory.Exists(backupFolderPath);
            if (dirExists)
                return "hg pull -u";

            string gitCommand = "hg clone";
            return $"{gitCommand} {cloneUrl} \"{backupFolderPath}\"";
        }
    }
}

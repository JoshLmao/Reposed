using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Utility
{
    public class DirectoryUtility
    {
        /// <summary>
        /// Gets the size in bytes of a directory
        /// </summary>
        /// <param name="directoryPath">The full directory path</param>
        /// <returns></returns>
        public static long GetDirectoryWithChildrenSize(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                //ToDo
                return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
            }
            else
                return -1;
        }

        public static double BytesToGB(long bytes)
        {
            return bytes / 1024.0 / 1024.0 / 1024.0;
        }

        public static void OpenFolderInExplorer(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Process.Start(filePath);
            }
        }
    }
}

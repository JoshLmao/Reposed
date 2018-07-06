using Caliburn.Micro;
using Reposed.MVVM;
using Reposed.ServiceComponents;
using Reposed.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.BackupInformation
{
    public class FolderInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }

        public double SizeGB { get; set; }
        public bool HasLFS { get; set; }
    }

    public class BackupInformationViewModel : ViewModelBase, IServiceComponent
    {
        public static string SERVICE_ID { get { return "BackupInfo"; } }

        List<FolderInfo> m_foldersInformation;
        public List<FolderInfo> FoldersInformation
        {
            get { return m_foldersInformation; }
            set
            {
                m_foldersInformation = value;
                NotifyOfPropertyChange(() => FoldersInformation);
            }
        }

        string m_currentGitPath;
        public string CurrentGitPath
        {
            get { return m_currentGitPath; }
            set
            {
                m_currentGitPath = value;
                NotifyOfPropertyChange(() => CurrentGitPath);
            }
        }

        string m_localBackupPath;
        public string LocalBackupPath
        {
            get { return m_localBackupPath; }
            set
            {
                m_localBackupPath = value;
                NotifyOfPropertyChange(() => LocalBackupPath);
            }
        }

        double m_directorySize;
        public double DirectorySize
        {
            get { return m_directorySize; }
            set
            {
                m_directorySize = value;
                NotifyOfPropertyChange(() => DirectorySize);
            }
        }

        public BackupInformationViewModel()
        {
            
        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            base.OnViewLoaded(e);

            var prefsVM = IoC.Get<Preferences.PreferencesViewModel>();
            CurrentGitPath = prefsVM.GitPath;
            LocalBackupPath = prefsVM.LocalBackupPath;

            long dirSizeBytes = DirectoryUtility.GetDirectoryWithChildrenSize(LocalBackupPath);

            DirectorySize = Math.Round(DirectoryUtility.BytesToGB(dirSizeBytes), 2);

            FoldersInformation = GetFoldersInfo(LocalBackupPath);
        }

        public void OnOpenBackupPathFolder()
        {
            if (System.IO.Directory.Exists(LocalBackupPath))
                System.Diagnostics.Process.Start(LocalBackupPath);
        }

        List<FolderInfo> GetFoldersInfo(string path)
        {
            if (!Directory.Exists(path))
                return null;

            List<FolderInfo> foldersInfo = new List<FolderInfo>();
            string[] folders = Directory.GetDirectories(path);
            foreach(string folderPath in folders)
            {
                long sizeBytes = DirectoryUtility.GetDirectoryWithChildrenSize(folderPath);
                FolderInfo info = new FolderInfo
                {
                    Name = new DirectoryInfo(folderPath).Name,
                    FullPath = folderPath,
                    SizeGB = Math.Round(DirectoryUtility.BytesToGB(sizeBytes), 2),
                };
                foldersInfo.Add(info);
            }

            return foldersInfo;
        }
    }
}

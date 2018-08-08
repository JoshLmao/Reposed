using Caliburn.Micro;
using Reposed.Core.Events;
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

    public class BackupInformationViewModel : ViewModelBase, IServiceComponent, IHandle<OnRepoBackupSucceeded>, IHandle<OnRepoBackupFailed>
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
                NotifyOfPropertyChange(() => HasFolders);
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

        public bool HasFolders { get { return FoldersInformation != null && FoldersInformation.Count > 0; } }

        readonly IEventAggregator EVENT_AGGREGATOR;

        public BackupInformationViewModel(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            base.OnViewLoaded(e);
            UpdateInfo();
        }

        void UpdateInfo()
        {
            Preferences.PreferencesViewModel prefsVM = IoC.Get<Preferences.PreferencesViewModel>();
            CurrentGitPath = prefsVM.GitPath;
            LocalBackupPath = prefsVM.LocalBackupPath;

            long dirSizeBytes = DirectoryUtility.GetDirectoryWithChildrenSize(LocalBackupPath);

            DirectorySize = Math.Round(DirectoryUtility.BytesToGB(dirSizeBytes), 2);

            List<FolderInfo> folderInfos = new List<FolderInfo>();
            IEnumerable<Core.IBackupService> s = IoC.GetAll<Core.IBackupService>();
            foreach (Core.IBackupService service in s)
            {
                string subFolder = Path.Combine(LocalBackupPath, service.ServiceId);
                List<FolderInfo> infos = GetFoldersInfo(subFolder);
                if(infos != null)
                    folderInfos.AddRange(infos);
            }
            FoldersInformation = folderInfos;
        }

        public void OnOpenBackupPathFolder()
        {
            DirectoryUtility.OpenFolderInExplorer(LocalBackupPath);
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

        public void OnOpenFolder(FolderInfo dataContext)
        {
            DirectoryUtility.OpenFolderInExplorer(dataContext.FullPath);
        }

        public void Handle(OnRepoBackupSucceeded message)
        {
            UpdateInfo();
        }

        public void Handle(OnRepoBackupFailed message)
        {
            UpdateInfo();
        }
    }
}

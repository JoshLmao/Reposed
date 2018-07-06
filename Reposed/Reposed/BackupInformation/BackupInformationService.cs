using Reposed.Core;
using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.BackupInformation
{
    /// <summary>
    /// Empty service to be used to show the BackupInformation View
    /// </summary>
    public class BackupInformationService : IBackupService
    {
        public static string SERVICE_ID { get { return "BackupService"; } }

        public string ServiceId { get { return SERVICE_ID; } }

        public bool IsAuthorized => true;
        public bool CanBackup => false;
        public int CompletedReposCount => 0;
        public int SucceededReposCount => 0;
        public int TotalReposCount => 0;

        public string CurrentBackedUpRepo => "";

        public event Action<bool> OnIsAuthorizedChanged;

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public bool Backup(string rootBackupDir)
        {
            return false;
        }

        public void Init(string gitFilePath)
        {
            throw new NotImplementedException();
        }

        public void SetBackupRepos(List<BackupReposDto> backupRepos)
        {
            throw new NotImplementedException();
        }

        public bool SetCredentials(IBackupSettings credentials)
        {
            return true;
        }

        public bool SetGitPath(string gitPath)
        {
            return true;
        }
    }
}

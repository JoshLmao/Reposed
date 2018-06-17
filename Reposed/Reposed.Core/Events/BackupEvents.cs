using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Events
{
    public class RunScheduledBackup
    {
        public RunScheduledBackup()
        {

        }
    }

    public class OnBackupCompleted
    {
    }

    public class OnRepoBackupSucceeded
    {
        public string Repo { get; set; }
        public IBackupService BackupService { get; set; }
        public OnRepoBackupSucceeded(string repoName, IBackupService service)
        {
            Repo = repoName;
            BackupService = service;
        }
    }

    public class OnRepoBackupFailed
    {
        public string Repo { get; set; }
        public IBackupService BackupService { get; set; }
        public OnRepoBackupFailed(string repoName, IBackupService service)
        {
            Repo = repoName;
            BackupService = service;
        }
    }

    public class OnRepoStartBackup
    {
        public string Repo { get; set; }
        public IBackupService BackupService { get; set; }
        public OnRepoStartBackup(string repoName, IBackupService service)
        {
            Repo = repoName;
            BackupService = service;
        }
    }
}

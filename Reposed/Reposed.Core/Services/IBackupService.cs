using Reposed.Models;

namespace Reposed.Core
{
    public interface IBackupService
    {
        /// <summary>
        /// The constant id of the service
        /// </summary>
        string ServiceId { get; }
        /// <summary>
        /// Does the service have valid credentials to perform it's tasks
        /// </summary>
        bool IsAuthorized { get; }
        /// <summary>
        /// Can this service run a backup?
        /// </summary>
        bool CanBackup { get; }
        /// <summary>
        /// The current amount of completed repositories backed up
        /// </summary>
        int CompletedReposCount { get; }
        /// <summary>
        /// The current amount of repositories backed up successfully
        /// </summary>
        int SucceededReposCount { get; }
        /// <summary>
        /// The total amount of repositories to back up
        /// </summary>
        int TotalReposCount { get; }
        /// <summary>
        /// The name of the current repo being backed up
        /// </summary>
        string CurrentBackedUpRepo { get; }

        event System.Action<bool> OnCanBackupChanged;
        event System.Action<bool> OnIsAuthorizedChanged;

        event System.Action<string> OnStartBackupRepo;
        event System.Action<string> OnFinishRepoBackedUp;

        /// <summary>
        /// Configures the service with the necessary params
        /// </summary>
        /// <param name="gitFilePath"></param>
        void Init(string gitFilePath);
        /// <summary>
        /// Backs up all repositories that are available
        /// </summary>
        /// <param name="rootBackupDir"></param>
        /// <returns></returns>
        bool Backup(string rootBackupDir);
        /// <summary>
        /// Set the service specific credentials with the settings from the prefs
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        bool SetCredentials(IBackupCredentials credentials);
        bool SetGitPath(string gitPath);
        void SetBackupRepos(System.Collections.Generic.List<BackupReposDto> backupRepos);
    }
}

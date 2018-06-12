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

        event System.Action<bool> OnCanBackupChanged;
        event System.Action<bool> OnIsAuthorizedChanged;

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
    }
}

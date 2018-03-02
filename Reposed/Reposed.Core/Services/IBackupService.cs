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

        bool SetCredentials(IBackupCredentials credentials);
    }
}

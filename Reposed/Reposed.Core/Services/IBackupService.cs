using Reposed.Models;

namespace Reposed.Core
{
    public interface IBackupService
    {
        bool IsValid { get; }

        bool Backup(string rootBackupDir);
        bool SetCredentials(IBackupCredentials credentials);
        void Init(string gitFilePath);
    }
}

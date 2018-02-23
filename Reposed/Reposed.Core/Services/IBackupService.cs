using Reposed.Models;

namespace Reposed.Core
{
    public interface IBackupService
    {
        bool IsValid { get; }

        bool Backup();
        bool SetCredentials(IBackupCredentials credentials);
    }
}

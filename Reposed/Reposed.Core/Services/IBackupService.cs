using Reposed.Models;

namespace Reposed.Core
{
    public interface IBackupService
    {
        bool Backup();
        bool SetCredentials(IBackupCredentials credentials);
    }
}

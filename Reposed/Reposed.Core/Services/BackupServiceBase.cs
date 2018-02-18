using NLog;
using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services
{
    public class BackupServiceBase :  IBackupService
    {
        protected readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public BackupServiceBase()
        {

        }

        public virtual bool Backup()
        {
            return false;
        }

        public virtual bool SetCredentials(IBackupCredentials credentials)
        {
            return false;
        }
    }
}

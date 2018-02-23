using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Bitbucket
{
    public class BitbucketBackupService : BackupServiceBase
    {
        public BitbucketBackupService() : base()
        {
            
        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            IsValid = credentials is GithubPrefs prefs;
            if (IsValid)
            {
                return true;
            }
            else
            {
                LOGGER.Info($"Not correct type");
                return false;
            }
        }
    }
}

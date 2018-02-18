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
            if (credentials is GithubPrefs prefs)
            {
                
                return true;
            }
            else
            {
                LOGGER.Info($"Unable to");
                return false;
            }
        }
    }
}

using NLog;
using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Github
{
    public class GithubBackupService : BackupServiceBase
    {
        public static string SERVICE_ID = "GITHUB";

        public override string ServiceId { get { return SERVICE_ID; } }

        public GithubBackupService() : base()
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

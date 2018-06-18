using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Reposed.Models;

namespace Reposed.Core.Services.Gitlab
{
    public class GitlabBackupService : BackupServiceBase
    {
        public static string SERVICE_ID { get { return "Gitlab"; } }

        public override string ServiceId { get { return SERVICE_ID; } }

        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }

        protected override string m_serviceNameFolder => throw new NotImplementedException();

        GitlabAPIService m_gitlabService = null;

        public GitlabBackupService(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }

        public override bool Backup(string rootBackupDir)
        {
            throw new NotImplementedException();
        }

        public override bool SetCredentials(IBackupSettings credentials)
        {
            if (credentials is GitlabSettings settings)
            {
                Username = settings.Username;
                PublicKey = settings.PublicKey;
                PrivateKey = settings.PrivateKey;

                m_gitlabService = new GitlabAPIService(Username, PublicKey, PrivateKey);

                InitWithCredentials(m_gitlabService.GetAllRepositories().Count);

                return true;
            }
            return false;
        }

        protected override string GetRepoCloneUrl(string repoName)
        {
            throw new NotImplementedException();
        }
    }
}

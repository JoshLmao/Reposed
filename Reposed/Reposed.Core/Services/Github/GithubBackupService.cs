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

        public string Token { get; private set; }

        public override event Action OnRepoBackedUp;

        GithubAPIService m_githubApiService = null;

        public GithubBackupService() : base()
        {

        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            if (credentials is GithubPrefs prefs)
            {
                Username = prefs.Username;
                Token = prefs.PublicKey;

                m_githubApiService = new GithubAPIService(prefs.Username, prefs.PublicKey);

                return true;
            }
            else
            {
                LOGGER.Info($"Unable to set GitHub credentials");
                return false;
            }
        }

        public List<Octokit.Repository> GetAllRepositories()
        {
            return m_githubApiService?.GetAllRepositories();
        }
    }
}

using Caliburn.Micro;
using NLog;
using Octokit;
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

        protected override string m_serviceNameFolder { get { return "Github"; } }

        GithubAPIService m_githubApiService = null;

        public GithubBackupService(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            if (credentials is GithubPrefs prefs)
            {
                Username = prefs.Username;
                Token = prefs.PublicKey;

                m_githubApiService = new GithubAPIService(prefs.Username, prefs.PublicKey);

                InitWithCredentials(m_githubApiService.GetAllRepositories().Count);
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

        public override bool Backup(string rootBackupDir)
        {
            if(!IsAuthorized)
            {
                LOGGER.Error("Unable to backup Github API");
                return false;
            }

            CanBackup = false;
            string currentRepoName = string.Empty;
            try
            {
                List<Repository> repos = m_githubApiService.GetAllRepositories();
                foreach(Repository repo in repos)
                {
                    BackupReposDto backupRepoConfig = m_backupRepos?.FirstOrDefault(x => x.RepositoryName == repo.Name);
                    if (backupRepoConfig != null && !backupRepoConfig.ShouldBackup)
                    {
                        continue;
                    }

                    OnRepoStartBackup(repo.Name);
                    currentRepoName = repo.Name;

                    if (BackupSingleRepository(rootBackupDir, repo.Name))
                    {
                        OnRepoBackupSucceeded(repo.Name);
                    }
                    else
                    {
                        LOGGER.Error($"Unable to backup {repo.Name}");
                        OnRepoBackupFailed(repo.Name);
                    }

                    CompletedReposCount++;
                }
            }
            catch(Exception e)
            {
                LOGGER.Fatal($"Unable to backup repository {currentRepoName}");
            }

            CanBackup = true;
            return true;
        }

        protected override string GetRepoCloneUrl(string repoName)
        {
            return m_githubApiService.GetRepoUrl(repoName);
        }
    }
}

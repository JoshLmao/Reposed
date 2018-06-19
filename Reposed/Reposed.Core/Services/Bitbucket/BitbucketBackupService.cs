using Caliburn.Micro;
using Reposed.Models;
using SharpBucket.V2.Pocos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Bitbucket
{
    public class BitbucketBackupService : BackupServiceBase
    {
        public static string SERVICE_ID = "BITBUCKET";

        public override string ServiceId { get { return SERVICE_ID; } }

        public BitbucketAPIService APIService { get { return m_bitbucketAPI; } }

        //Authentification
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }

        public List<BackupReposDto> ReposToBackup { get; set; }

        protected override string m_serviceNameFolder { get { return "Bitbucket"; } }

        BitbucketAPIService m_bitbucketAPI = null;

        public BitbucketBackupService(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public override bool SetCredentials(IBackupSettings credentials)
        {
            if (credentials is BitBucketSettings)
            {
                BitBucketSettings bbPrefs = credentials as BitBucketSettings;
                Username = bbPrefs.Username;
                PublicKey = bbPrefs.PublicKey;
                PrivateKey = bbPrefs.PrivateKey;

                m_bitbucketAPI = new BitbucketAPIService(Username, PublicKey, PrivateKey);
                int count = m_bitbucketAPI.IsAuthorized() ? m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username).Count : 0;
                InitializeWithCredentials(count);
                return true;
            }
            else
            {
                LOGGER.Info($"Not correct credentials type");
                IsAuthorized = CanBackup = false;
                return false;
            }
        }

        public override bool Backup(string rootBackupDir)
        {
            if (!IsAuthorized)
            {
                LOGGER.Error("Unable to backup Bitbucket API");
                return false;
            }

            OnStartAllBackups();

            CanBackup = false;
            string currentRepoName = null;
            string serviceSubFolderDir = Path.Combine(rootBackupDir, m_serviceNameFolder);
            try
            {
                List<Repository> repos = m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username);
                foreach (Repository repo in repos)
                {
                    BackupReposDto backupRepoConfig = m_backupRepos?.FirstOrDefault(x => x.RepositoryName == repo.name);
                    if (backupRepoConfig != null && !backupRepoConfig.ShouldBackup)
                        continue;

                    OnRepoStartBackup(repo.name);
                    currentRepoName = repo.name;

                    if (BackupSingleRepository(serviceSubFolderDir, repo.name))
                    {
                        OnRepoBackupSucceeded(repo.name);
                    }
                    else
                    {
                        LOGGER.Error($"Unable to backup {repo.name}");
                        OnRepoBackupFailed(repo.name);
                    }

                    CompletedReposCount++;
                }
            }
            catch (Exception e)
            {
                LOGGER.Fatal($"Exception occured when trying to backup repository '{currentRepoName}'{Environment.NewLine}{e.ToString()}");
            }

            OnCompletedAllBackups();

            CanBackup = true;
            return true;
        }

        protected override string GetRepoCloneUrl(string repoName)
        {
            return m_bitbucketAPI.GetRepoUrl(repoName, false);
        }

        public List<Repository> GetAllRepositories()
        {
            return m_bitbucketAPI?.GetAllRepos(m_bitbucketAPI.Username);
        }

        public override bool IsServiceAuthorized()
        {
            return m_bitbucketAPI.IsAuthorized();
        }
    }
}

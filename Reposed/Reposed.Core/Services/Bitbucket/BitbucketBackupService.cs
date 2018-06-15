using Reposed.Models;
using SharpBucket.V2.Pocos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Bitbucket
{
    public class BitbucketBackupService : BackupServiceBase
    {
        public static string SERVICE_ID = "BITBUCKET";

        public override event Action OnRepoBackedUp;

        public override string ServiceId { get { return SERVICE_ID; } }

        public BitbucketAPIService APIService { get { return m_bitbucketAPI; } }

        //Authentification
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }

        public List<BackupReposDto> ReposToBackup { get; set; }

        BitbucketAPIService m_bitbucketAPI = null;

        public BitbucketBackupService() : base()
        {
        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            if (credentials is BitBucketPrefs)
            {
                BitBucketPrefs bbPrefs = credentials as BitBucketPrefs;
                Username = bbPrefs.Username;
                PublicKey = bbPrefs.PublicKey;
                PrivateKey = bbPrefs.PrivateKey;

                m_bitbucketAPI = new BitbucketAPIService(Username, PublicKey, PrivateKey);

                InitWithCredentials(m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username).Count);
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

            CanBackup = false;
            string currentRepoName = null;
            try
            {
                List<Repository> repos = m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username);
                foreach (Repository repo in repos)
                {
                    BackupReposDto backupRepoConfig = m_backupRepos?.FirstOrDefault(x => x.RepositoryName == repo.name);
                    if (backupRepoConfig != null && !backupRepoConfig.ShouldBackup)
                    {
                        continue;
                    }

                    currentRepoName = repo.name;

                    if (BackupSingleRepository(rootBackupDir, repo.name, repo))
                    {
                        SucceededReposCount++;
                    }
                    else
                    {
                        LOGGER.Error($"Unable to backup {repo.name}");
                    }

                    CompletedReposCount++;
                    OnRepoBackedUp?.Invoke();
                }
            }
            catch (Exception e)
            {
                LOGGER.Fatal($"Unable to backup repository {currentRepoName}");
            }

            CanBackup = true;
            return true;
        }

        protected override string GetGitCommand(object repo, string folderPath)
        {
            Repository repository = repo as Repository;

            string command = "";
            string gitCommand = "";
            string repoUrl = m_bitbucketAPI.GetRepoUrl(repository.name, false);
            bool dirExists = System.IO.Directory.Exists(folderPath);

            //Check if we have credentials for ssh, else use https
            if (repository.scm == "hg")
            {
                //need to test
                if (dirExists)
                    return "hg pull -u";
                else
                    gitCommand = "hg clone";

                command = $"{gitCommand} {repoUrl} \"{folderPath}\"";
            }
            else if (repository.scm == "git")
            {
                if (dirExists)
                    return $"remote update";
                else
                    gitCommand = "clone --mirror";

                command = $"{gitCommand} {repoUrl} \"{folderPath}\"";
            }

            return command;
        }

        public List<Repository> GetAllRepositories()
        {
            return m_bitbucketAPI?.GetAllRepos(m_bitbucketAPI.Username);
        }
    }
}

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

        public override string ServiceId { get { return SERVICE_ID; } }

        public BitbucketAPIService APIService { get { return m_bitbucketAPI; } }

        BitbucketAPIService m_bitbucketAPI = null;

        public BitbucketBackupService() : base()
        {
        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            if (credentials is BitBucketPrefs)
            {
                BitBucketPrefs bbPrefs = credentials as BitBucketPrefs;
                m_bitbucketAPI = new BitbucketAPIService(bbPrefs.Username, bbPrefs.PublicKey, bbPrefs.PrivateKey);
                IsAuthorized = CanBackup = true;

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
                LOGGER.Error("Unable to backup Bitbucket API. m_bitbucketAPI is null");
                return false;
            }

            CanBackup = false;
            string currentRepoName = null;
            try
            {
                List<Repository> repos = m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username);
                foreach(Repository repo in repos)
                {
                    currentRepoName = repo.name;

                    if (!BackupRepo(rootBackupDir, repo))
                        LOGGER.Error($"Unable to backup {repo.name}");
                }
            }
            catch (Exception e)
            {
                LOGGER.Fatal($"Unable to backup repository {currentRepoName}");
            }

            CanBackup = false;
            return true;
        }

        bool BackupRepo(string rootBackupDir, Repository repo)
        {
            System.Threading.Thread.Sleep(1000);
            return true;

            string currentRepoDir = $"{rootBackupDir}\\{repo.name}";
            string command = GetGitCommand(repo, currentRepoDir);
            PrepareRepoDirectory(currentRepoDir);

            bool cmdSuccess = ExecuteGitCommand(command);
            if (!cmdSuccess)
                LOGGER.Error($"Error executing Git command on repository '{repo.name}'");

            return cmdSuccess;
        }

        string GetGitCommand(Repository repo, string folderPath)
        {
            string command = "";
            string gitCommand = "";
            string repoUrl = m_bitbucketAPI.GetRepoUrl(repo.name, false);

            //Check if we have credentials for ssh, else use https
            bool dirExists = System.IO.Directory.Exists(folderPath);
            if (repo.scm == "hg")
            {
                //need to test
                if (dirExists)
                    return "hg pull -u";
                else
                    gitCommand = "hg clone";

                command = $"{gitCommand} {repoUrl} \"{folderPath}\"";
            }
            else if (repo.scm == "git")
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
            return m_bitbucketAPI.GetAllRepos(m_bitbucketAPI.Username);
        }
    }
}

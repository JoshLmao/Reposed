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

        BitbucketAPIService m_bitbucketAPI = null;

        public BitbucketBackupService() : base()
        {
        }

        public override bool SetCredentials(IBackupCredentials credentials)
        {
            if (credentials is BitBucketPrefs)
            {
                BitBucketPrefs prefs = credentials as BitBucketPrefs;
                m_bitbucketAPI = new BitbucketAPIService(prefs.Username, prefs.PublicKey, prefs.PrivateKey);
                IsAuthorized = true;

                return true;
            }
            else
            {
                LOGGER.Info($"Not correct credentials type");
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

            return true;
        }

        bool BackupRepo(string rootBackupDir, Repository repo)
        {
            string currentRepoDir = $"{rootBackupDir}\\{repo.name}";
            string command = GetGitCommand(repo, currentRepoDir);
            PrepareRepoDirectory(currentRepoDir);

            bool cmdSuccess = ExecuteGitCommand(command);
            if (!cmdSuccess)
                LOGGER.Error($"Error updating repository '{repo.name}'");

            return true;
        }

        string GetGitCommand(Repository repo, string folderPath)
        {
            string command = "";
            string gitCommand = "";
            string repoUrl = m_bitbucketAPI.GetRepoUrl(repo.name, true);

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
    }
}

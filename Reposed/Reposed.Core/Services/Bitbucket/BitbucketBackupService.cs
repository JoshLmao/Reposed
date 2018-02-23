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
        readonly BitbucketAPIService BITBUCKET_API = null;

        public BitbucketBackupService() : base()
        {
            BITBUCKET_API = new BitbucketAPIService(null, null);
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

        public override bool Backup(string rootBackupDir)
        {
            string currentRepoName = null;
            try
            {
                List<Repository> repos = BITBUCKET_API.GetAllRepos(null);
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
            string repoUrl = BITBUCKET_API.GetRepoUrl(repo.name, true);

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

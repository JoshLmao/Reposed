using NLog;
using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services
{
    public abstract class BackupServiceBase : IBackupService
    {
        public abstract string ServiceId { get; }

        bool m_isAuthorized;
        public bool IsAuthorized
        {
            get { return m_isAuthorized; }
            protected set
            {
                m_isAuthorized = value;
                OnIsAuthorizedChanged?.Invoke(IsAuthorized);
            }
        }

        bool m_canBackup = false;
        public bool CanBackup
        {
            get { return m_canBackup; }
            protected set
            {
                m_canBackup = value;
                OnCanBackupChanged?.Invoke(CanBackup);
            }
        }

        public int CompletedReposCount { get; protected set; }
        public int SucceededReposCount { get; protected set; }
        public int TotalReposCount { get; protected set; }

        public string Username { get; protected set; }

        public event Action<bool> OnIsAuthorizedChanged;
        public event Action<bool> OnCanBackupChanged;
        public abstract event Action OnRepoBackedUp;

        protected List<BackupReposDto> m_backupRepos = null;

        protected readonly Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        string m_gitFilePath = null;

        public BackupServiceBase()
        {

        }

        public virtual bool Backup(string rootBackupDir)
        {
            return false;
        }

        public virtual bool SetCredentials(IBackupCredentials credentials)
        {
            return false;
        }

        public bool SetGitPath(string gitPath)
        {
            if (File.Exists(gitPath))
            {
                //ToDo: Do more validation to check it's actually the git exe
                m_gitFilePath = gitPath;
                return true;
            }
            return false;
        }

        public void Init(string gitFilePath)
        {
            m_gitFilePath = gitFilePath;
        }

        /// <summary>
        /// Creates and sets the working directory to the current repo
        /// </summary>
        /// <param name="repoDirectory">The path to the current repo to create</param>
        protected void PrepareRepoDirectory(string repoDirectory)
        {
            if (!Directory.Exists(repoDirectory))
            {
                Directory.CreateDirectory(repoDirectory);
            }

            Environment.CurrentDirectory = repoDirectory;
        }

        /// <summary>
        /// Backs up a single repository using 
        /// </summary>
        /// <param name="rootBackupDir">The root folder where all repos will be backed up to</param>
        /// <param name="repoName">The name of the repo</param>
        /// <param name="repository">The rpeo object from the relevent API</param>
        /// <returns>If the backup was successful or not</returns>
        protected virtual bool BackupSingleRepository(string rootBackupDir, string repoName, object repository)
        {
            //For testing
            //System.Threading.Thread.Sleep(1000);
            //return true;

            string currentRepoDir = $"{rootBackupDir}\\{repoName}";
            string command = GetGitCommand(repository, currentRepoDir);
            PrepareRepoDirectory(currentRepoDir);

            bool cmdSuccess = ExecuteGitCommand(command);
            if (!cmdSuccess)
                LOGGER.Error($"Error executing Git command on repository '{repoName}'");

            return cmdSuccess;
        }

        /// <summary>
        /// Returns the correct Git command to execute to backup the repo locally.
        /// </summary>
        /// <param name="repo">The repository object with relevent information</param>
        /// <param name="folderPath">The formatted full path where the repo will be backed up to</param>
        /// <returns>The full Git command to execute</returns>
        protected abstract string GetGitCommand(object repo, string folderPath);

        /// <summary>
        /// Executes a specific git command and closes
        /// </summary>
        /// <param name="fileName">The filePath to git</param>
        /// <param name="command">the git command</param>
        /// <returns>If the program exited successfully or not</returns>
        protected bool ExecuteGitCommand(string command)
        {
            if(string.IsNullOrEmpty(m_gitFilePath))
            {
                LOGGER.Error("Unable to execute git command. GitFilePath is null");
                return false;
            }

            Process gitProc = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                Arguments = command,
                FileName = m_gitFilePath,

                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            gitProc.StartInfo = startInfo;
            gitProc.Start();

            //Wait for git to process it's command before leaving
            gitProc.WaitForExit();
            
            return gitProc.ExitCode == 0;
        }

        protected void InitWithCredentials(int repoCount)
        {
            IsAuthorized = CanBackup = true;

            TotalReposCount = repoCount;
            CompletedReposCount = SucceededReposCount = 0;
        }

        public virtual void SetBackupRepos(List<BackupReposDto> backupRepos)
        {
            m_backupRepos = backupRepos;
        }
    }
}

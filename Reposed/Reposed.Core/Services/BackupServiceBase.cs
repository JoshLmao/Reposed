﻿using NLog;
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
    public class BackupServiceBase : IBackupService
    {
        public bool IsValid { get; protected set; } = true;

        protected readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

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
                UseShellExecute = false,
                FileName = m_gitFilePath,
            };
            gitProc.StartInfo = startInfo;
            gitProc.Start();

            //Wait for git to process it's command before leaving
            gitProc.WaitForExit();
            
            return gitProc.ExitCode == 0;
        }
    }
}

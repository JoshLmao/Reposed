using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using Reposed.Core;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reposed.Preferences
{
    public class PreferencesViewModel : ViewModelBase
    {
        string m_gitPath = null;
        public string GitPath
        {
            get { return m_gitPath; }
            set
            {
                m_gitPath = value;
                NotifyOfPropertyChange(() => GitPath);
            }
        }

        string m_localBackupPath;
        public string LocalBackupPath
        {
            get { return m_localBackupPath; }
            set
            {
                m_localBackupPath = value;
                NotifyOfPropertyChange(() => LocalBackupPath);
            }
        }

        #region BitbucketProperties
        string m_bb_username;
        public string BB_Username
        {
            get { return m_bb_username; }
            set
            {
                m_bb_username = value;
                NotifyOfPropertyChange(() => BB_Username);
            }
        }

        string m_bb_OAuthPublicKey;
        public string BB_OAuthPublicKey
        {
            get { return m_bb_OAuthPublicKey; }
            set
            {
                m_bb_OAuthPublicKey = value;
                NotifyOfPropertyChange(() => BB_OAuthPublicKey);
            }
        }

        string m_bb_OAuthPrivateKey;
        public string BB_OAuthPrivateKey
        {
            get { return m_bb_OAuthPrivateKey; }
            set
            {
                m_bb_OAuthPrivateKey = value;
                NotifyOfPropertyChange(() => BB_OAuthPrivateKey);
            }
        }
        #endregion

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        Models.Preferences m_prefs = null;

        public PreferencesViewModel(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public void LoadPreferences()
        {
            if(File.Exists(FilePathUtility.PreferencesFilePath))
            {
                string json = File.ReadAllText(FilePathUtility.PreferencesFilePath);
                m_prefs = JsonConvert.DeserializeObject<Models.Preferences>(json);

                if (m_prefs == null)
                {
                    m_prefs = GetDefaultPrefs();
                }
                else
                {
                    //Set VM properties from loaded prefs
                    GitPath = m_prefs.LocalGitPath;
                    LocalBackupPath = m_prefs.LocalBackupPath;

                    Models.BitBucketPrefs bbPrefs = m_prefs.BitbucketPrefs;
                    if (bbPrefs != null)
                    {
                        BB_OAuthPublicKey = bbPrefs.PublicKey;
                        BB_OAuthPrivateKey = bbPrefs.PrivateKey;
                        BB_Username = bbPrefs.Username;
                    }
                    else
                        LOGGER.Info("No Bitbucket Preferences found");

                    SetServiceData();
                }
            }
            else
            {
                //Default prefs if none exists
                m_prefs = GetDefaultPrefs();

                SaveToFile(m_prefs);
            }

            EVENT_AGGREGATOR.PublishOnCurrentThread(new PreferencesUpdated(m_prefs));
        }

        public void OnSelectGitPath()
        {
            GitPath = ShowOpenFileDialog(".exe", "Select Git.exe");
        }

        public void OnSelectedBackupPath()
        {
            LocalBackupPath = Path.GetDirectoryName(ShowOpenFileDialog("", "Select Root Folder (Select a file inside the folder for now)"));
        }

        string ShowOpenFileDialog(string defaultExt, string title)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = defaultExt,
                Title = title,
                Multiselect = false,
            };

            if (ofd.ShowDialog() == true)
            {
                return ofd.FileName;
            }
            return null;
        }

        public void OnApply()
        {
            m_prefs.LocalGitPath = GitPath;
            m_prefs.LocalBackupPath = LocalBackupPath;

            m_prefs.BitbucketPrefs = new Models.BitBucketPrefs()
            {
                Username = BB_Username,
                PublicKey = BB_OAuthPublicKey,
                PrivateKey = BB_OAuthPrivateKey,
            };

            SetServiceData();

            EVENT_AGGREGATOR.PublishOnCurrentThread(new PreferencesUpdated(m_prefs));

            SaveToFile(m_prefs);
            TryClose(true);
        }

        public void OnCancel()
        {
            TryClose(false);
        }

        Models.Preferences GetDefaultPrefs()
        {
            return new Models.Preferences()
            {

            };
        }

        void SaveToFile(Models.Preferences prefs)
        {
            if (!Directory.Exists(FilePathUtility.LocalAppDataFolder))
                Directory.CreateDirectory(FilePathUtility.LocalAppDataFolder);

            if (!File.Exists(FilePathUtility.PreferencesFilePath))
                File.Create(FilePathUtility.PreferencesFilePath).Close();

            string json = JsonConvert.SerializeObject(m_prefs, Formatting.Indented);
            File.WriteAllText(FilePathUtility.PreferencesFilePath, json);
        }

        public Models.Preferences GetPreferences()
        {
            return m_prefs;
        }

        /// <summary>
        /// Gets all IBackupService's and sets all needed data from prefs
        /// </summary>
        void SetServiceData()
        {
            IEnumerable<IBackupService> backups = IoC.GetAll<IBackupService>();
            foreach (IBackupService service in backups)
            {
                service.SetGitPath(m_prefs.LocalGitPath);

                Models.IBackupCredentials creds = null;
                if(service.ServiceId == Core.Services.Bitbucket.BitbucketBackupService.SERVICE_ID)
                    creds = m_prefs.BitbucketPrefs;
                else if(service.ServiceId == Core.Services.Github.GithubBackupService.SERVICE_ID)
                    creds = m_prefs.GithubPrefs;
                else
                    throw new NotImplementedException();

                service.SetCredentials(creds);
            }
        }
    }
}

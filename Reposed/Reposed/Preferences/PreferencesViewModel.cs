using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using Reposed.Core;
using Reposed.Core.Dialogs;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.Services.Plugins;
using Reposed.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reposed.Preferences
{
    public class PreferencesViewModel : ViewModelBase
    {
        public override string DisplayName { get { return "Preferences"; } set { } }

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

        #region SlackBot Info Properties
        public string Name
        {
            get { return m_prefs?.SlackBotInfo?.Name; }
            set
            {
                m_prefs.SlackBotInfo.Name = value;
                NotifyOfPropertyChange(() => Name);
                SetSlackBotFromSettings();
            }
        }

        public string Token
        {
            get { return m_prefs?.SlackBotInfo?.Token; }
            set
            {
                m_prefs.SlackBotInfo.Token = value;
                NotifyOfPropertyChange(() => Token);
                SetSlackBotFromSettings();
            }
        }

        public string ChannelName
        {
            get { return m_prefs?.SlackBotInfo?.Channel; }
            set
            {
                m_prefs.SlackBotInfo.Channel = value;
                NotifyOfPropertyChange(() => ChannelName);
                SetSlackBotFromSettings();
            }
        }
        #endregion

        public bool IsSlackBotActive { get { return SLACK_SERVICE.IsEnabled; } }
        public string SlackBotActiveText { get { return IsSlackBotActive ? "Active" : "Disabled"; } }

        public bool IsSlackBotChannelValid
        {
            get { return SLACK_SERVICE != null ? SLACK_SERVICE.HasValidChannel : false; }
        }

        readonly IEventAggregator EVENT_AGGREGATOR = null;
        readonly IMessageBoxService MSG_BOX_SERVICE = null;
        readonly SlackService SLACK_SERVICE = null;

        Models.Preferences m_prefs = null;

        public PreferencesViewModel(IEventAggregator eventAggregator, IMessageBoxService msgBoxService, SlackService slackService)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);

            MSG_BOX_SERVICE = msgBoxService;
            SLACK_SERVICE = slackService;
            SLACK_SERVICE.OnBotConnectionChanged += OnSlackBotConnected;
            SLACK_SERVICE.OnBotChannelChanged += OnSlackBotChannelChanged;
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
                    if (m_prefs.SlackBotInfo == null)
                    {
                        m_prefs.SlackBotInfo = new Models.Plugins.SlackBotInfo();
                    }
                    else
                    {
                        SLACK_SERVICE.Set(m_prefs.SlackBotInfo);
                    }
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
            string newPath = ShowOpenFileDialog(".exe", "Select Git.exe");
            string fileName = Path.GetFileName(newPath);
            if (File.Exists(newPath) && fileName.ToLower() == "git.exe")
            {
                GitPath = newPath;
            }
            else
            {
                MSG_BOX_SERVICE.Show($"Unable to set Git Path to {newPath}", "Error selecting directory", System.Windows.MessageBoxButton.OK);
            }
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
                Filter = "Git .exe file (*.exe)|*.exe|All files (*.*)|*.*"
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

            EVENT_AGGREGATOR.PublishOnCurrentThread(new PreferencesUpdated(m_prefs));

            if(SLACK_SERVICE != null && !SLACK_SERVICE.IsConnected)
                SetSlackBotFromSettings();

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
                SlackBotInfo = new Models.Plugins.SlackBotInfo()
                {

                }
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

        void SetSlackBotFromSettings()
        {
            if (SLACK_SERVICE != null)
            {
                //We need these properties, no need to update if they aren't there/valid
                //if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(ChannelName))
                //    return;

                SLACK_SERVICE.Set(new Models.Plugins.SlackBotInfo()
                {
                    Name = Name,
                    Token = Token,
                    Channel = ChannelName,
                });

                NotifyOfPropertyChange(() => IsSlackBotActive);
            }
        }

        private void OnSlackBotConnected(bool connected)
        {
            NotifyOfPropertyChange(() => IsSlackBotActive);
            NotifyOfPropertyChange(() => IsSlackBotChannelValid);
            NotifyOfPropertyChange(() => SlackBotActiveText);
        }

        private void OnSlackBotChannelChanged(bool validChannel, string channelName)
        {
            NotifyOfPropertyChange(() => IsSlackBotActive);
            NotifyOfPropertyChange(() => IsSlackBotChannelValid);
            NotifyOfPropertyChange(() => SlackBotActiveText);
        }
    }
}

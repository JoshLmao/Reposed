using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    m_prefs = GetDefaultPrefs();
                else
                {
                    GitPath = m_prefs.LocalGitPath;
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
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = ".exe",
                Title = "Select Git .exe",
                Multiselect = false,
            };

            if(ofd.ShowDialog() == true)
            {
                GitPath = ofd.FileName;
            }
        }

        public void OnApply()
        {
            m_prefs.LocalGitPath = GitPath;

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
    }
}

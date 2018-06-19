using Caliburn.Micro;
using Newtonsoft.Json;
using NLog;
using Reposed.Events;
using Reposed.Models;
using Reposed.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Services
{
    public class BackupSettingsService : IHandle<OnApplicationClosing>
    {
        public List<IBackupSettings> BackupSettings { get; private set; }

        readonly IEventAggregator EVENT_AGGREGATOR = null;
        readonly Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public BackupSettingsService(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public void Load(List<IBackupSettings> settings)
        {
            BackupSettings = settings;
        }

        public IBackupSettings Get(string serviceId)
        {
            return BackupSettings?.FirstOrDefault(x => x.ServiceId == serviceId);
        }

        public void LoadFromFile()
        {
            if (File.Exists(FilePathUtility.BackupSettingsFilePath))
            {
                string json = File.ReadAllText(FilePathUtility.BackupSettingsFilePath);
                List<IBackupSettings> backupSettings = JsonConvert.DeserializeObject<List<IBackupSettings>>(json, new Core.Json.BackupSettingsConverter());
                if(backupSettings != null)
                {
                    BackupSettings = backupSettings;
                }
                else
                {
                    LOGGER.Error("Unable to load backup settings from file");
                    SetDefaultSettings();
                }
            }
            else
            {
                SetDefaultSettings();
            }

            List<Core.IBackupService> services = IoC.GetAll<Core.IBackupService>().ToList();
            foreach (Core.IBackupService service in services)
            {
                bool result = service.SetCredentials(BackupSettings.FirstOrDefault(x => x.ServiceId == service.ServiceId));
                if (!result)
                    throw new NotImplementedException($"Given incorrect settings type to service {service.ServiceId}");
            }
        }

        private void SetDefaultSettings()
        {
            BackupSettings = new List<IBackupSettings>()
            {
                new BitBucketSettings(Core.Services.Bitbucket.BitbucketBackupService.SERVICE_ID),
                new GithubSettings(Core.Services.Github.GithubBackupService.SERVICE_ID),
                new GitlabSettings(Core.Services.Gitlab.GitlabBackupService.SERVICE_ID),
            };
        }

        public void SaveToFile()
        {
            string savePath = FilePathUtility.BackupSettingsFilePath;
            if (!Directory.Exists(FilePathUtility.LocalAppDataFolder))
                Directory.CreateDirectory(FilePathUtility.LocalAppDataFolder);

            if (!File.Exists(savePath))
                File.Create(savePath).Close();

            string json = JsonConvert.SerializeObject(BackupSettings, Formatting.Indented);
            File.WriteAllText(savePath, json);
        }

        public void Set(string serviceId, IBackupSettings serviceSettings)
        {
            if(BackupSettings != null)
            {
                IBackupSettings existingSettings = BackupSettings.FirstOrDefault(x => x.ServiceId == serviceId);
                if(existingSettings != null)
                    BackupSettings.Remove(existingSettings);

                BackupSettings.Add(serviceSettings);
                SaveToFile();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Handle(OnApplicationClosing message)
        {
            SaveToFile();
        }
    }
}

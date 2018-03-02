using Caliburn.Micro;
using Reposed.Core;
using Reposed.Events;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.BackupController
{
    public class BackupControllerViewModel : ViewModelBase, IHandle<PreferencesUpdated>
    {
        List<IBackupService> m_backupServices;
        public List<IBackupService> BackupServices
        {
            get { return m_backupServices; }
            set
            {
                m_backupServices = value;
                NotifyOfPropertyChange(() => BackupServices);
            }
        }

        IBackupService m_selectedBackupService;
        public IBackupService SelectedBackupService
        {
            get { return m_selectedBackupService; }
            set
            {
                m_selectedBackupService = value;
                NotifyOfPropertyChange(() => SelectedBackupService);
            }
        }

        public string BackupPath { get; set; }

        protected string m_rootBackupFolder;

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        public BackupControllerViewModel(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public void OnViewLoaded(ActionExecutionContext e)
        {
            UpdateServices();
        }

        void UpdateServices()
        {
            BackupServices.Clear();

            List<IBackupService> services = IoC.GetAll<IBackupService>().ToList();
            foreach (IBackupService service in services)
            {
                if (service.IsValid)
                {
                    BackupServices.Add(service);
                }
            }
        }

        public void OnStartBackups()
        {
            if (SelectedBackupService == null)
            {
                LOGGER.Error("Unable to backup. No selected backup service");
                return;
            }

            if (SelectedBackupService.IsValid)
                SelectedBackupService.Backup(m_rootBackupFolder);
            else
                LOGGER.Error($"Unable to backup service. Isn't valid");
        }

        public void Handle(PreferencesUpdated message)
        {
            BackupPath = message.Prefs.LocalBackupPath;
            NotifyOfPropertyChange(() => BackupPath);
        }
    }
}

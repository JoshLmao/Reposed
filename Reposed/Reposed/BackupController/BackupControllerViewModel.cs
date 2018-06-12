using Caliburn.Micro;
using Reposed.Core;
using Reposed.Events;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.BackupController
{
    public class BackupControllerViewModel : ViewModelBase, IHandle<PreferencesUpdated>, IHandle<OnAccountSelected>
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
                if(SelectedBackupService != null)
                {
                    SelectedBackupService.OnCanBackupChanged -= OnCanBackupChanged;
                    SelectedBackupService.OnRepoBackedUp -= OnRepoBackedUp;
                    //SelectedBackupService.OnIsAuthorizedChanged -= OnIsAuthorizedChanged;
                }

                m_selectedBackupService = value;
                NotifyOfPropertyChange(() => SelectedBackupService);

                OnSelectedBackupServiceChanged();
            }
        }

        private void OnSelectedBackupServiceChanged()
        {
            if (SelectedBackupService != null)
            {
                SelectedBackupService.OnCanBackupChanged += OnCanBackupChanged;
                SelectedBackupService.OnRepoBackedUp += OnRepoBackedUp;
                //SelectedBackupService.OnIsAuthorizedChanged += OnIsAuthorizedChanged;
            }

            NotifyOfPropertyChange(() => CanBackup);
            OnRepoBackedUp();
        }

        public bool CanBackup
        {
            get { return SelectedBackupService != null ? SelectedBackupService.CanBackup : false; }
        }

        public string BackupPath { get; set; }

        public string ProgressText
        {
            get { return $"Progress: Completed = {SelectedBackupService?.CompletedReposCount}, Succeeded = {SelectedBackupService?.SucceededReposCount}, TotalRepos = {SelectedBackupService?.TotalReposCount}"; }
        }

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        private Thread m_backupThread = null;

        public BackupControllerViewModel(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            UpdateServices();
        }

        void UpdateServices()
        {
            if (BackupServices == null)
                BackupServices = new List<IBackupService>();

            BackupServices.Clear();

            List<IBackupService> services = IoC.GetAll<IBackupService>().ToList();
            foreach (IBackupService service in services)
            {
                BackupServices.Add(service);
            }

            SelectedBackupService = BackupServices.FirstOrDefault();
        }

        public void OnStartBackups()
        {
            if (SelectedBackupService == null)
            {
                LOGGER.Error("Unable to backup. No selected backup service");
                return;
            }

            if(m_backupThread != null)
            {
                LOGGER.Info("Backup in progress.");
                return;
            }
            if (SelectedBackupService.IsAuthorized)
            {
                ThreadStart start = new ThreadStart(StartBackup);
                start += () =>
                {
                    //Callback once finished
                    m_backupThread = null;
                };
                m_backupThread = new Thread(start);
                m_backupThread.Start();
            }
            else
            {
                LOGGER.Error($"Unable to backup service. Isn't valid");
            }
        }

        void StartBackup()
        {
            SelectedBackupService.Backup(BackupPath);
        }

        public void Handle(PreferencesUpdated message)
        {
            BackupPath = message.Prefs.LocalBackupPath;
            NotifyOfPropertyChange(() => BackupPath);
        }

        public void Handle(OnAccountSelected message)
        {
            SelectedBackupService = message.Service;
        }

        private void OnCanBackupChanged(bool canBackup)
        {
            NotifyOfPropertyChange(() => CanBackup);
        }

        private void OnRepoBackedUp()
        {
            NotifyOfPropertyChange(() => ProgressText);
        }
    }
}

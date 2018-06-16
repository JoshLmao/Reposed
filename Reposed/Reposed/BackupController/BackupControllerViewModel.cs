using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Events;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.BackupController
{
    public class BackupControllerViewModel : ViewModelBase, IHandle<PreferencesUpdated>, IHandle<OnAccountSelected>, IHandle<RunScheduledBackup>, IHandle<OnBackupCompleted>
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
                OnSelectedBackupServicePreChanged();

                m_selectedBackupService = value;
                NotifyOfPropertyChange(() => SelectedBackupService);

                OnSelectedBackupServiceChanged();
            }
        }

        DateTime m_lastBackupStartTime;
        public DateTime LastBackupStartTime
        {
            get { return m_lastBackupStartTime; }
            set
            {
                m_lastBackupStartTime = value;
                NotifyOfPropertyChange(() => LastBackupStartTime);
                NotifyOfPropertyChange(() => TotalTimeToBackup);
            }
        }

        DateTime m_lastBackupEndTime;
        public DateTime LastBackupEndTime
        {
            get { return m_lastBackupEndTime; }
            set
            {
                m_lastBackupEndTime = value;
                NotifyOfPropertyChange(() => LastBackupEndTime);
                NotifyOfPropertyChange(() => TotalTimeToBackup);
            }
        }

        int m_backupProgressPercentageValue;
        public int BackupProgressPercentageValue
        {
            get { return m_backupProgressPercentageValue; }
            set
            {
                m_backupProgressPercentageValue = value;
                NotifyOfPropertyChange(() => BackupProgressPercentageValue);
            }
        }

        public DateTime NextScheduledBackupTime
        {
            get { return m_scheduledBackupService != null ? m_scheduledBackupService.NextScheduledBackupTime : DateTime.MinValue; }
        }

        public string TotalTimeToBackup { get { return LastBackupEndTime != DateTime.MinValue && LastBackupStartTime != DateTime.MinValue ? (LastBackupEndTime - LastBackupStartTime).ToString() : "?"; } }
        public bool IsScheduledEnabled { get { return m_scheduledBackupService.IsEnabled; } }

        public bool CanBackup
        {
            get
            {
                if (m_scheduledBackupService.IsEnabled)
                    return false;
                if (SelectedBackupService != null)
                    return SelectedBackupService.CanBackup;
                return false;
            }
        }

        public string BackupPath { get; set; }

        public string ProgressText
        {
            get { return $"Progress: Completed = {SelectedBackupService?.CompletedReposCount}, Succeeded = {SelectedBackupService?.SucceededReposCount}, TotalRepos = {SelectedBackupService?.TotalReposCount}"; }
        }

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        private Thread m_backupThread = null;
        private ScheduledBackupService m_scheduledBackupService = null;

        public BackupControllerViewModel(IEventAggregator eventAggregator, ScheduledBackupService scheduledBackupService)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);

            m_scheduledBackupService = scheduledBackupService;
        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            UpdateServices();
            NotifyOfPropertyChange(() => IsScheduledEnabled);
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

        public void OnBackupNow()
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
            LastBackupStartTime = DateTime.Now;
            SelectedBackupService.Backup(BackupPath);
            LastBackupEndTime = DateTime.Now;

            EVENT_AGGREGATOR.PublishOnCurrentThread(new OnBackupCompleted());
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

        private void OnSelectedBackupServicePreChanged()
        {
            if (SelectedBackupService != null)
            {
                SelectedBackupService.OnCanBackupChanged -= OnCanBackupChanged;
                SelectedBackupService.OnRepoBackedUp -= OnRepoBackedUp;
                //SelectedBackupService.OnIsAuthorizedChanged -= OnIsAuthorizedChanged;
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

        private void OnCanBackupChanged(bool canBackup)
        {
            NotifyOfPropertyChange(() => CanBackup);
        }

        private void OnRepoBackedUp()
        {
            NotifyOfPropertyChange(() => ProgressText);
        }

        public void OnConfigureAutoBackup()
        {
            IoC.Get<IWindowManager>().ShowDialog(IoC.Get<Dialogs.ScheduledBackup.ScheduledBackupViewModel>());

            NotifyOfPropertyChange(() => NextScheduledBackupTime);
            NotifyOfPropertyChange(() => IsScheduledEnabled);
            NotifyOfPropertyChange(() => CanBackup);
        }

        public void Handle(RunScheduledBackup message)
        {
            OnBackupNow();
            NotifyOfPropertyChange(() => NextScheduledBackupTime);
        }

        public void OnCancelScheduleBackup()
        {
            m_scheduledBackupService.Disable();
        }

        public void Handle(OnBackupCompleted message)
        {
            if (m_scheduledBackupService.IsEnabled)
            {
                m_scheduledBackupService.Resume();
            }

            NotifyOfPropertyChange(() => NextScheduledBackupTime);
        }
    }
}

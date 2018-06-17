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
    public class BackupControllerViewModel : ViewModelBase, IHandle<PreferencesUpdated>, IHandle<OnAccountSelected>, IHandle<RunScheduledBackup>, IHandle<OnBackupCompleted>,
        IHandle<OnRepoBackupFailed>, IHandle<OnRepoBackupSucceeded>, IHandle<OnRepoStartBackup>
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

        bool m_isBackingUp = false;
        public bool IsBackingUp
        {
            get { return m_isBackingUp; }
            set
            {
                m_isBackingUp = value;
                NotifyOfPropertyChange(() => IsBackingUp);
            }
        }

        private string m_currentStatusText;
        public string CurrentStatusText
        {
            get { return m_currentStatusText; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_currentStatusText = m_scheduledBackupService.IsEnabled ? "Waiting for scheduled backup..." :  "Idle";
                }
                else
                    m_currentStatusText = value;
                NotifyOfPropertyChange(() => CurrentStatusText);
            }
        }

        public string BackupPath { get; set; }

        public string ProgressText
        {
            get { return $"Progress: Completed = {BackupServices?.Sum(x => x.CompletedReposCount)}, Succeeded = {BackupServices?.Sum(x => x.SucceededReposCount)}, TotalRepos = {BackupServices?.Sum(x => x.TotalReposCount)}"; }
        }

        readonly IEventAggregator EVENT_AGGREGATOR = null;
        readonly IWindowManager WINDOW_MANAGER = null;

        private Thread m_backupThread = null;
        private Thread m_backupCompleteMsgChangeThread = null;
        private ScheduledBackupService m_scheduledBackupService = null;

        public BackupControllerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, ScheduledBackupService scheduledBackupService)
        {
            WINDOW_MANAGER = windowManager;
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);

            m_scheduledBackupService = scheduledBackupService;
        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            UpdateServices();
            NotifyOfPropertyChange(() => IsScheduledEnabled);
            //Refresh status text
            CurrentStatusText = "";
        }

        void UpdateServices()
        {
            if (BackupServices == null)
                BackupServices = new List<IBackupService>();

            if(BackupServices?.Count > 0)
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
                ThreadStart start = new ThreadStart(RunBackup);
                m_backupThread = new Thread(start);
                m_backupThread.Start();
            }
            else
            {
                LOGGER.Error($"Unable to backup service. Isn't valid");
            }
        }

        private void RunBackup()
        {
            IsBackingUp = true;
            LastBackupStartTime = DateTime.Now;

            foreach(IBackupService service in BackupServices)
                service.Backup(BackupPath);

            LastBackupEndTime = DateTime.Now;
            IsBackingUp = false;

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
                //SelectedBackupService.OnIsAuthorizedChanged -= OnIsAuthorizedChanged;
            }
        }

        private void OnSelectedBackupServiceChanged()
        {
            if (SelectedBackupService != null)
            {
                SelectedBackupService.OnCanBackupChanged += OnCanBackupChanged;
                //SelectedBackupService.OnIsAuthorizedChanged += OnIsAuthorizedChanged;
            }

            NotifyOfPropertyChange(() => CanBackup);
            NotifyOfPropertyChange(() => ProgressText);
        }

        private void OnCanBackupChanged(bool canBackup)
        {
            NotifyOfPropertyChange(() => CanBackup);
        }

        private void OnRepoBackedUp(string repoName)
        {
        }

        public void OnConfigureAutoBackup()
        {
            WINDOW_MANAGER.ShowDialog(IoC.Get<Dialogs.ScheduledBackup.ScheduledBackupViewModel>());

            NotifyOfPropertyChange(() => NextScheduledBackupTime);
            NotifyOfPropertyChange(() => IsScheduledEnabled);
            NotifyOfPropertyChange(() => CanBackup);
            //Refresh status text
            CurrentStatusText = "";
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
            m_backupThread = null;

            if (m_scheduledBackupService.IsEnabled)
            {
                m_scheduledBackupService.Resume();
            }

            if(m_backupCompleteMsgChangeThread == null)
            {
                CurrentStatusText = "Completed all backups";
                ThreadStart tStart = new ThreadStart(ChangeToIdleDelay);
                m_backupCompleteMsgChangeThread = new Thread(tStart);
                m_backupCompleteMsgChangeThread.Start();
            }

            NotifyOfPropertyChange(() => ProgressText);
            NotifyOfPropertyChange(() => NextScheduledBackupTime);
        }

        private void ChangeToIdleDelay()
        {
            Thread.Sleep(10 * 1000);
            CurrentStatusText = "";
            m_backupCompleteMsgChangeThread = null;
        }

        public void OnCancelBackup()
        {
            if(IsBackingUp)
            {
                m_backupThread.Abort();
                m_backupThread = null;
                IsBackingUp = false;

                EVENT_AGGREGATOR.PublishOnCurrentThread(new OnBackupCompleted());

                LOGGER.Info("Cancelling current backup");
            }
        }

        public void Handle(OnRepoBackupSucceeded message)
        {
            NotifyOfPropertyChange(() => ProgressText);
            CurrentStatusText = $"Finished backup of '{message.Repo}'";
        }

        public void Handle(OnRepoBackupFailed message)
        {
            NotifyOfPropertyChange(() => ProgressText);
            CurrentStatusText = $"Failed backup of '{message.Repo}'";
        }

        public void Handle(OnRepoStartBackup message)
        {
            NotifyOfPropertyChange(() => ProgressText);
            CurrentStatusText = $"Starting backup of '{message.Repo}'";
        }
    }
}

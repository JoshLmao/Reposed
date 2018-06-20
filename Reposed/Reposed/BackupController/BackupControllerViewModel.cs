﻿using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Dialogs;
using Reposed.Core.Events;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.Services;
using Reposed.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.BackupController
{
    public class BackupControllerViewModel : ViewModelBase, IHandle<PreferencesUpdated>, IHandle<OnAccountSelected>, IHandle<RunScheduledBackup>, IHandle<OnBackupFinished>,
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

        TimeSpan m_totalBackupTime;
        public TimeSpan TotalBackupTime
        {
            get { return m_totalBackupTime; }
            set
            {
                m_totalBackupTime = value;
                NotifyOfPropertyChange(() => TotalBackupTime);
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

        public string BackupPath { get { return m_prefs?.LocalBackupPath; } }

        public string ProgressText
        {
            get { return $"Progress: Completed = {BackupServices?.Sum(x => x.CompletedReposCount)}, Succeeded = {BackupServices?.Sum(x => x.SucceededReposCount)}, TotalRepos = {BackupServices?.Sum(x => x.TotalReposCount)}"; }
        }

        readonly IEventAggregator EVENT_AGGREGATOR = null;
        readonly IWindowManager WINDOW_MANAGER = null;
        readonly IMessageBoxService MSG_BOX_SERVICE = null;
        readonly SlackService SLACK_SERVICE = null;

        private Models.Preferences m_prefs = null;
        private Thread m_backupThread = null;
        private Thread m_backupCompleteMsgChangeThread = null;
        private ScheduledBackupService m_scheduledBackupService = null;

        public BackupControllerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, IMessageBoxService msgBoxService, ScheduledBackupService scheduledBackupService, SlackService slackService)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
            WINDOW_MANAGER = windowManager;
            MSG_BOX_SERVICE = msgBoxService;
            SLACK_SERVICE = slackService;

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
            if (PreBackupChecks(true))
            {
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
        }

        private bool PreBackupChecks(bool showMsgBoxDialog)
        {
            if (SelectedBackupService == null)
            {
                LOGGER.Error("Unable to backup. No selected backup service");
                return false;
            }

            if (m_backupThread != null)
            {
                if (showMsgBoxDialog)
                    MSG_BOX_SERVICE.Show("A backup is already in progress, can't start another until the previous one is finished", "Backup in progress...");

                LOGGER.Info("Backup in progress.");
                return false;
            }

            if (string.IsNullOrEmpty(m_prefs?.LocalBackupPath))
            {
                if (showMsgBoxDialog)
                    MSG_BOX_SERVICE.Show("The backup path hasn't been set", "Backup Path not set");

                LOGGER.Error("Can't backup as backup path hasn't been set");
                return false;
            }

            if (string.IsNullOrEmpty(m_prefs?.LocalGitPath))
            {
                if (showMsgBoxDialog)
                    MSG_BOX_SERVICE.Show("The Git executable path hasn't been set", "Git path not set");

                LOGGER.Error("Git path hasn't been set");
                return false;
            }
            return true;
        }

        private void RunBackup()
        {
            IsBackingUp = true;
            LastBackupStartTime = DateTime.Now;

            foreach(IBackupService service in BackupServices)
                service.Backup(BackupPath);

            EVENT_AGGREGATOR.PublishOnCurrentThread(new OnBackupFinished(true));
        }

        public void Handle(PreferencesUpdated message)
        {
            m_prefs = message.Prefs;
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

        public void Handle(OnBackupFinished message)
        {
            m_backupThread = null;

            if (m_scheduledBackupService.IsEnabled)
            {
                m_scheduledBackupService.Resume();
            }

            LastBackupEndTime = DateTime.Now;
            TotalBackupTime = LastBackupEndTime - LastBackupStartTime;
            IsBackingUp = false;

            if (m_backupCompleteMsgChangeThread == null)
            {
                CurrentStatusText = message.IsSuccessful ? "Completed all backups" : "Cancelled backup";
                ThreadStart tStart = new ThreadStart(ChangeToIdleDelay);
                m_backupCompleteMsgChangeThread = new Thread(tStart);
                m_backupCompleteMsgChangeThread.Start();
            }

            NotifyOfPropertyChange(() => ProgressText);
            NotifyOfPropertyChange(() => NextScheduledBackupTime);

            if(SLACK_SERVICE != null)
            {
                SLACK_SERVICE.SendBackupMessage(new SlackService.BackupInfo()
                {
                    IsSuccessful = message.IsSuccessful,
                    TotalBackupTime = TotalBackupTime,
                });
            }
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

                SelectedBackupService.Abort();

                EVENT_AGGREGATOR.PublishOnCurrentThread(new OnBackupFinished(false));

                LOGGER.Info("Current backup cancelled by user");
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

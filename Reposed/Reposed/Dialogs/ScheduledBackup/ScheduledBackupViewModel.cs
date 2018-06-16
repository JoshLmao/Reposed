using Reposed.Core.Enums;
using Reposed.Core.Services;
using Reposed.MVVM;
using Reposed.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Dialogs.ScheduledBackup
{
    public class ScheduledBackupViewModel : ViewModelBase
    {
        public override string DisplayName { get { return "Backup Scheduler"; } set { } }

        int m_backupPeriodAmount = 1;
        public int BackupPeriodAmount
        {
            get { return m_backupPeriodAmount; }
            set
            {
                m_backupPeriodAmount = value;
                NotifyOfPropertyChange(() => BackupPeriodAmount);
            }
        }

        BackupPeriod m_selectedBackupPeriod = BackupPeriod.Days;
        public BackupPeriod SelectedBackupPeriod
        {
            get { return m_selectedBackupPeriod; }
            set
            {
                m_selectedBackupPeriod = value;
                NotifyOfPropertyChange(() => SelectedBackupPeriod);
            }
        }

        public bool IsScheduledEnabled { get { return m_backupService.IsEnabled; } }
        public string SchedulerStatus { get { return m_backupService.IsEnabled ? "Enabled" : "Disabled"; } }

        ScheduledBackupService m_backupService;

        public ScheduledBackupViewModel(ScheduledBackupService backupService)
        {
            m_backupService = backupService;
        }

        public void OnViewLoaded()
        {
            Update();
        }

        public void OnSetScheduled()
        {
            m_backupService.Enable(BackupPeriodAmount, SelectedBackupPeriod);
            Update();

            TryClose(true);
        }

        public void OnCancelScheduled()
        {
            TryClose(false);
        }

        public void OnCancelScheduleBackup()
        {
            m_backupService.Disable();
            Update();
        }

        void Update()
        {
            NotifyOfPropertyChange(() => SchedulerStatus);
            NotifyOfPropertyChange(() => IsScheduledEnabled);
        }
    }
}

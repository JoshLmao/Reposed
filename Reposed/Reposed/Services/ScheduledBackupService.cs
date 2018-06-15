using Caliburn.Micro;
using Reposed.Core.Enums;
using Reposed.Core.Events;
using System;
using System.Timers;

namespace Reposed.Services
{
    public class ScheduledBackupService
    {
        public DateTime NextScheduledBackupTime { get; private set; }

        public bool IsEnabled { get; private set; }

        private Timer m_scheduleTimer;
        private int m_currentPeriodAmount = 0;
        private BackupPeriod m_currentPeriodType = BackupPeriod.Minutes;

        readonly IEventAggregator EVENT_AGGREGATOR;
        readonly NLog.Logger LOGGER;

        public ScheduledBackupService(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            LOGGER = NLog.LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Enabled the scheduler to do auto backups using the given settings
        /// </summary>
        /// <param name="backupPeriodAmount"></param>
        /// <param name="selectedBackupPeriod"></param>
        public void Enable(int backupPeriodAmount, BackupPeriod selectedBackupPeriod)
        {
            m_currentPeriodAmount = backupPeriodAmount;
            m_currentPeriodType = selectedBackupPeriod;

            DestroyTimer();

            double interval = 0.0;
            switch (selectedBackupPeriod)
            {
                case BackupPeriod.Days:
                    interval = DaysToMillis(backupPeriodAmount);
                    break;
                case BackupPeriod.Hours:
                    interval = HoursToMillis(backupPeriodAmount); //Hours to milliseconds
                    break;
                case BackupPeriod.Minutes:
                    interval = MinutesToMillis(backupPeriodAmount); //Seconds to milliseconds
                    break;
                default:
                    throw new NotImplementedException();
            }

            m_scheduleTimer = new Timer(interval);
            m_scheduleTimer.Elapsed += OnRunScheduledBackup;
            m_scheduleTimer.Start();

            IsEnabled = true;

            NextScheduledBackupTime = DateTime.Now.Add(TimeSpan.FromMilliseconds(interval));
        }

        /// <summary>
        /// Stops the scheduler from doing auto backups
        /// </summary>
        public void Disable()
        {
            DestroyTimer();
            NextScheduledBackupTime = DateTime.MinValue;
            IsEnabled = false;
            LOGGER.Info("Disabled scheduled backup");
        }

        void DestroyTimer()
        {
            if (m_scheduleTimer != null)
            {
                m_scheduleTimer.Stop();
                m_scheduleTimer.Dispose();
                m_scheduleTimer.Elapsed -= OnRunScheduledBackup;
                m_scheduleTimer = null;
            }
        }

        private void OnRunScheduledBackup(object sender, ElapsedEventArgs e)
        {
            LOGGER.Info("Starting scheduled backup");

            DestroyTimer();
            EVENT_AGGREGATOR.PublishOnCurrentThread(new RunScheduledBackup());
        }

        double DaysToMillis(int days)
        {
            return HoursToMillis(days * 24);
        }

        double HoursToMillis(double hours)
        {
            return SecondsToMilliseconds(hours * 60);
        }

        double MinutesToMillis(double minutes)
        {
            return SecondsToMilliseconds(minutes * 60);
        }

        double SecondsToMilliseconds(double seconds)
        {
            return seconds * 1000;
        }

        /// <summary>
        /// Resumes the scheduler using the last settings
        /// </summary>
        public void Resume()
        {
            if(m_currentPeriodAmount > 0)
                Enable(m_currentPeriodAmount, m_currentPeriodType);
        }
    }
}

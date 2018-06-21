using Caliburn.Micro;
using Reposed.Core.Plugins;
using Reposed.Events;
using Reposed.Plugins.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.Services.Plugins
{
    public class SlackService : IPluginService, IHandle<OnApplicationClosing>
    {
        public class BackupInfo
        {
            public bool IsSuccessful { get; set; }
            public TimeSpan TotalBackupTime { get; set; }
        }

        public bool IsConnected { get { return m_bot != null ? m_bot.IsConnected : false; } }
        public bool HasValidChannel { get { return m_bot != null ? m_bot.HasValidChannel : false; } }
        public bool IsEnabled { get { return IsConnected && HasValidChannel; } }

        public string SuccessfulHexColor { get; set; } = "00FF13";
        public string FailedHexColor { get; set; } = "FF0000";

        public event Action<bool> OnBotConnectionChanged;
        public event Action<bool, string> OnBotChannelChanged;

        SlackBot m_bot = null;
        Thread m_channelThread = null;

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        public SlackService(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public void Set(Models.Plugins.SlackBotInfo info)
        {
            if(m_bot != null)
            {
                m_bot.Dispose();
                m_bot.OnConnected -= OnBotConnectedToSlack;
                OnBotConnectionChanged?.Invoke(false);
            }

            m_bot = new SlackBot(info.Token, info.Name);
            m_bot.OnConnected += OnBotConnectedToSlack;

            //Force the client to constantly check for the channel & force connect
            //ToDo: Make this async and improve
            if (m_channelThread != null)
                m_channelThread.Abort();
            m_channelThread = new Thread(() => FindChannel(info.Channel));
            m_channelThread.Start();
        }

        private void FindChannel(string channelName)
        {
            while(!m_bot.HasValidChannel)
            {
                Thread.Sleep(5 * 1000);
                m_bot.SetChannel(channelName);
            }

            OnBotChannelChanged?.Invoke(m_bot.HasValidChannel, channelName);
        }

        private void OnBotConnectedToSlack()
        {
            OnBotConnectionChanged?.Invoke(true);
        }

        public void SendMessage(string message, string hexColor, List<KeyValuePair<string, string>> fieldsInfo)
        {
            m_bot.SendFormatMessage(message, hexColor, fieldsInfo);
        }

        public void SendBackupMessage(BackupInfo info)
        {
            string msg = "?";
            string hexColor = "";
            List<KeyValuePair<string, string>> fields = null;
            if (info.IsSuccessful)
            {
                msg = $"Successfully backed up repositories";
                hexColor = SuccessfulHexColor;
                fields = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Status", "Successful"),
                    new KeyValuePair<string, string>("Total Time", info.TotalBackupTime.ToString()),
                };
            }
            else
            {
                msg = $"Failed to backup up repositories";
                hexColor = FailedHexColor;
                fields = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Status", "Failed"),
                    new KeyValuePair<string, string>("Total Time", info.TotalBackupTime.ToString()),
                    //new KeyValuePair<string, string>("Reason", "ToDo"),
                };
            }

            SendMessage(msg, hexColor, fields);
        }

        public void Handle(OnApplicationClosing message)
        {
            if (message.IsUnexpected)
            {
                List<KeyValuePair<string, string>> details = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Close time", DateTime.Now.ToString()),
                };

                if (message.Exception != null)
                    details.Add(new KeyValuePair<string, string>("Exception", message.Exception.ToString()));

                SendMessage("Reposed shut down unexpectedly", FailedHexColor, details);
            }
        }
    }
}

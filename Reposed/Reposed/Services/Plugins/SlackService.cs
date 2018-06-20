using Reposed.Core.Plugins;
using Reposed.Plugins.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.Services.Plugins
{
    public class SlackService : IPluginService
    {
        public class BackupInfo
        {
            public bool IsSuccessful { get; set; }
            public TimeSpan TotalBackupTime { get; set; }
        }

        public bool IsConnected { get { return m_bot.IsConnected; } }
        public bool HasValidChannel { get { return m_bot.HasValidChannel; } }

        public string SuccessfulHexColor { get; set; } = "00FF13";
        public string FailedHexColor { get; set; } = "FF0000";

        public event Action<bool> OnBotConnectionChanged;
        public event Action<bool, string> OnBotChannelChanged;

        SlackBot m_bot = null;
        Thread m_channelThread = null;

        public SlackService()
        {

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

            OnBotChannelChanged?.Invoke(m_bot.HasValidChannel, info.Channel);
        }

        private void FindChannel(string channelName)
        {
            while(!m_bot.HasValidChannel)
            {
                m_bot.SetChannel(channelName);
                Thread.Sleep(500);
            }
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
    }
}

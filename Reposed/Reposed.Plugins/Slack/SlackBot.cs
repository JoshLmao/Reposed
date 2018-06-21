using SlackAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reposed.Plugins.Slack
{
    public class SlackBot
    {
        public bool IsConnected { get; private set; }

        public bool HasValidChannel { get { return m_messageChannel != null; } }

        public event Action OnConnected;

        private string m_channelName = null;
        private string m_name = null;
        private string m_token = null;

        private SlackClient m_client = null;
        private Channel m_messageChannel = null;
        private object m_connectedLock = new object();

        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly string BOT_ICON_URL = "";

        public SlackBot(string token, string botName)
        {
            m_name = botName;
            m_token = token;

            m_client = new SlackClient(m_token);
            Connect();
        }

        public void SetChannel(string channelName)
        {
            m_channelName = channelName;

            if (m_client == null || m_client != null && m_client.Channels == null)
            {
                return;
            }

            Channel newChannel = m_client.Channels.Find(x => x.name.ToLower() == m_channelName.ToLower());
            if(newChannel == null)
            {
                LOGGER.Error($"Can't find channel with name '{channelName}'");
            }
            else
            {
                m_messageChannel = newChannel;
                LOGGER.Info($"Slack bot channel set to {m_channelName}");
            }
        }

        public void Dispose()
        {
            //if(m_client.IsConnected)
            //    m_client.CloseSocket();
            m_client.EmitLogin(null);
            if (IsConnected)
                IsConnected = false;

            m_client = null;
        }

        public bool SendFormatMessage(string message, string hexColor, List<KeyValuePair<string, string>> infoFields)
        {
            Attachment[] attc = GetAttachments(hexColor, infoFields);
            return SendMessage(message, hexColor, attc);
        }

        bool SendMessage(string message, string hexColor, Attachment[] attachments)
        {
            if (m_messageChannel != null)
            {
                m_client.PostMessage((msg) => { }, m_messageChannel.id, message, m_name, null, true, attachments, false, BOT_ICON_URL, null, false, null);
                return true;
            }
            else
            {
                LOGGER.Error("Channel has not been configured");
                return false;
            }
        }

        private bool Connect()
        {
            if (m_client != null)
            {
                m_client.Connect(x => OnClientConnected());
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnClientConnected()
        {
            if (IsConnected)
                return;

            lock (m_connectedLock)
            {
                LOGGER.Info($"Successfully connected Slack bot");
                IsConnected = true;
                OnConnected?.Invoke();
            }
        }

        private Attachment[] GetAttachments(string hexColor, List<KeyValuePair<string, string>> infoFields)
        {
            if (infoFields == null)
                return null;

            Field[] fields = infoFields.Select(x => new Field()
            {
                title = x.Key,
                value = x.Value,
            }).ToArray();

            return new Attachment[]
            {
                new Attachment()
                {
                    color = hexColor,
                    fields = fields,
                },
            };
        }
    }
}

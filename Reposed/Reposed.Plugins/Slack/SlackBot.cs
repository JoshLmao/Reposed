﻿using SlackAPI;
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
        private string m_channelName = null;
        private string m_name = null;
        private string m_token = null;

        private SlackSocketClient m_client = null;
        private Channel m_messageChannel = null;

        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public SlackBot(string token, string botName)
        {
            m_name = botName;
            m_token = token;

            m_client = new SlackSocketClient(m_token);
            Connect();
        }

        public void SetChannel(string channelName)
        {
            m_channelName = channelName;

            if (!m_client.IsConnected)
                return;

            Channel newChannel = m_client.Channels.Find(x => x.name == m_channelName);
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

        }

        public bool SendFormatMessage(string message, string hexColor, List<KeyValuePair<string, string>> infoFields)
        {
            Attachment[] attc = GetAttachments(message, hexColor, infoFields);
            return SendMessage(message, hexColor, attc);
        }

        bool SendMessage(string message, string hexColor, Attachment[] attachments)
        {
            if (!m_client.IsConnected)
                Connect();

            if (!string.IsNullOrEmpty(m_channelName))
            {
                m_client.PostMessage((msg) => { }, m_messageChannel.id, message, m_name, null, true, attachments, false, null, null, false, null);
                return true;
            }
            else
            {
                LOGGER.Error("Channel has not been configured");
                return false;
            }
        }

        bool Connect()
        {
            m_client.Connect(x => OnClientConnected());

            return m_client.IsConnected;
        }

        void OnClientConnected()
        {
            LOGGER.Info($"Successfully connected Slack bot");

            if (!string.IsNullOrEmpty(m_channelName) && m_messageChannel == null)
                SetChannel(m_channelName);
        }

        Attachment[] GetAttachments(string message, string hexColor, List<KeyValuePair<string, string>> infoFields)
        {
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
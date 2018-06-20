using Reposed.Core.Plugins;
using Reposed.Plugins.Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        SlackBot m_bot = null;

        public string SuccessfulHexColor { get; set; } = "00FF13";
        public string FailedHexColor { get; set; } = "FF0000";

        public SlackService()
        {

        }

        public void Set(Models.Plugins.SlackBotInfo info)
        {
            m_bot = new SlackBot(info.Token, info.Name);
            m_bot.SetChannel(info.Channel);
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

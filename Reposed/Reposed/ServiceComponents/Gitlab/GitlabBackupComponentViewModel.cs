using Caliburn.Micro;
using Reposed.Core.Services.Gitlab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Gitlab
{
    public class GitlabBackupComponentViewModel : ServiceComponentsBase
    {
        string m_username;
        public string Username
        {
            get { return m_username; }
            set
            {
                m_username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        GitlabBackupService GitlabService { get { return m_service as GitlabBackupService; } }

        public GitlabBackupComponentViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
        }
    }
}

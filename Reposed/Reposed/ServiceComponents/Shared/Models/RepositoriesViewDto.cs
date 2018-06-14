using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Shared.Models
{
    public class RepositoriesViewDto : PropertyChangedBase
    {
        string m_repoName;
        public string RepoName
        {
            get { return m_repoName; }
            set
            {
                m_repoName = value;
                NotifyOfPropertyChange(() => RepoName);
            }
        }

        bool m_shouldBackup = true;
        public bool ShouldBackup
        {
            get { return m_shouldBackup; }
            set
            {
                m_shouldBackup = value;
                NotifyOfPropertyChange(() => ShouldBackup);
            }
        }
    }
}

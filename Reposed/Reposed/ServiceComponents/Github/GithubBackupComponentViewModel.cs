using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Services.Github;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Github
{
    public class GithubBackupComponentViewModel : ServiceComponentsBase
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

        ObservableCollection<string> m_repositories;
        public ObservableCollection<string> Repositories
        {
            get { return m_repositories; }
            set
            {
                m_repositories = value;
                NotifyOfPropertyChange(() => Repositories);
            }
        }

        GithubBackupService GithubService { get { return m_service as GithubBackupService; } }

        bool m_hasInit = false;

        public GithubBackupComponentViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            m_service = IoC.GetAll<IBackupService>().FirstOrDefault(x => x is GithubBackupService);
        }

        public void OnViewLoaded()
        {
            if (!m_hasInit)
            {
                UpdateUI();

                m_hasInit = true;
            }
        }

        void UpdateUI()
        {
            if (GithubService != null)
            {
                Username = GithubService.Username;
                //OAuthPublic = BitbucketService.PublicKey;
                //OAuthPrivate = BitbucketService.PrivateKey;

                Repositories = new ObservableCollection<string>();
                var repos = GithubService.GetAllRepositories();
                if (repos != null)
                {
                    foreach (var repo in repos)
                    {
                        Repositories.Add(repo.Name);
                    }
                }
            }
        }
    }
}

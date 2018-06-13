using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Services.Bitbucket;
using Reposed.Events;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Bitbucket
{
    public class BitbucketBackupComponentViewModel : ServiceComponentsBase
    {
        public override string DisplayName { get { return "Bitbucket Backup View"; } set { } }

        public string m_username;
        public string Username
        {
            get { return m_username; }
            set
            {
                m_username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        string m_oathPublic;
        public string OAuthPublic
        {
            get { return m_oathPublic; }
            set
            {
                m_oathPublic = value;
                NotifyOfPropertyChange(() => OAuthPublic);
            }
        }

        string m_oauthPrivate;
        public string OAuthPrivate
        {
            get { return m_oauthPrivate; }
            set
            {
                m_oauthPrivate = value;
                NotifyOfPropertyChange(() => OAuthPrivate);
            }
        }

        ObservableCollection<string> m_repositories = null;
        public ObservableCollection<string> Repositories
        {
            get { return m_repositories; }
            set
            {
                m_repositories = value;
                NotifyOfPropertyChange(() => Repositories);
            }
        }

        BitbucketBackupService BitbucketService { get { return m_service as BitbucketBackupService; } }

        bool m_hasInit = false;

        public BitbucketBackupComponentViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            m_service = IoC.GetAll<IBackupService>().FirstOrDefault(x => x is BitbucketBackupService);
            BitbucketService.OnIsAuthorizedChanged += OnBitbucketAuthorizationChanged;
        }

        public void OnViewLoaded()
        {
            if(!m_hasInit)
            {
                UpdateUI();

                m_hasInit = false;
            }
        }

        public override void Handle(PreferencesUpdated message)
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            if (BitbucketService != null)
            {
                Username = BitbucketService.Username;
                OAuthPublic = BitbucketService.PublicKey;
                OAuthPrivate = BitbucketService.PrivateKey;

                Repositories = new ObservableCollection<string>();
                var repos = BitbucketService.GetAllRepositories();
                if(repos != null)
                {
                    foreach (var repo in repos)
                    {
                        Repositories.Add(repo.name);
                    }
                }
            }
        }

        private void OnBitbucketAuthorizationChanged(bool isAuthorized)
        {
            UpdateUI();
        }
    }
}

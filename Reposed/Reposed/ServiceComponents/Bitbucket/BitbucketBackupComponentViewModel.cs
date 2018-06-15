using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Services.Bitbucket;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.ServiceComponents.Shared.Models;
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

        BitbucketBackupService BitbucketService { get { return m_service as BitbucketBackupService; } }

        bool m_hasInit = false;

        public BitbucketBackupComponentViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            m_service = IoC.GetAll<IBackupService>().FirstOrDefault(x => x is BitbucketBackupService);
            BitbucketService.OnIsAuthorizedChanged += OnBitbucketAuthorizationChanged;
        }

        public void OnViewLoaded()
        {
            if (!m_hasInit)
            {
                UpdateUI();

                m_hasInit = true;
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

                Repositories = new ObservableCollection<RepositoriesViewDto>();
                var repos = BitbucketService.GetAllRepositories();
                if(repos != null)
                {
                    foreach (SharpBucket.V2.Pocos.Repository repo in repos)
                    {
                        Repositories.Add(new RepositoriesViewDto() { RepoName = repo.name });
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

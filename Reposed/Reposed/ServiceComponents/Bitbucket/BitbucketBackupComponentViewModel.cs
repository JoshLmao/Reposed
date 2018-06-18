using Caliburn.Micro;
using Newtonsoft.Json.Linq;
using Reposed.Core;
using Reposed.Core.Services.Bitbucket;
using Reposed.Events;
using Reposed.Models;
using Reposed.MVVM;
using Reposed.ServiceComponents.Shared.Models;
using Reposed.Services;
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

        public BitbucketBackupComponentViewModel(IEventAggregator eventAggregator, BackupSettingsService settingsService) : base(eventAggregator, settingsService)
        {
            GetService<BitbucketBackupService>();
        }

        protected override void UpdateUI()
        {
            if (BACKUP_SETTINGS_SERVICE != null)
            {
                m_serviceSettings = BACKUP_SETTINGS_SERVICE.Get(BitbucketBackupService.SERVICE_ID);
                BitBucketSettings bbSettings = m_serviceSettings as BitBucketSettings;

                Username = bbSettings.Username;
                OAuthPublic = bbSettings.PublicKey;
                OAuthPrivate = bbSettings.PrivateKey;

                Repositories = new ObservableCollection<RepositoriesViewDto>();
                List<SharpBucket.V2.Pocos.Repository> repos = BitbucketService.GetAllRepositories();
                if(repos != null)
                {
                    foreach (SharpBucket.V2.Pocos.Repository repo in repos)
                    {
                        JObject obj = JObject.Parse(repo.owner);
                        string ownerName = obj["username"].ToString();
                        string profilePicUrl = obj["links"]["avatar"]["href"].ToString();

                        Repositories.Add(new RepositoriesViewDto()
                        {
                            RepoName = repo.name,
                            Owner = ownerName,
                            PicUrl = profilePicUrl,
                        });
                    }
                }
            }
        }

        public override void OnApplySettingChanges()
        {
            SetCredentials(BitbucketBackupService.SERVICE_ID, new BitBucketSettings(BitbucketBackupService.SERVICE_ID)
            {
                Username = Username,
                PublicKey = OAuthPublic,
                PrivateKey = OAuthPrivate,
            });
        }
    }
}

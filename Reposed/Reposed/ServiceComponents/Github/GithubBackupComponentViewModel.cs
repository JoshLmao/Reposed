using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Services.Github;
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

        string m_tokenKey;
        public string TokenKey
        {
            get { return m_tokenKey; }
            set
            {
                m_tokenKey = value;
                NotifyOfPropertyChange(() => TokenKey);
            }
        }

        GithubBackupService GithubService { get { return m_service as GithubBackupService; } }

        public GithubBackupComponentViewModel(IEventAggregator eventAggregator, BackupSettingsService settingsService) : base(eventAggregator, settingsService)
        {
            m_service = IoC.GetAll<IBackupService>().FirstOrDefault(x => x is GithubBackupService);
        }

        protected override void UpdateUI()
        {
            if (BACKUP_SETTINGS_SERVICE != null)
            {
                m_serviceSettings = BACKUP_SETTINGS_SERVICE.Get(GithubBackupService.SERVICE_ID);
                GithubSettings bbSettings = m_serviceSettings as GithubSettings;

                Username = bbSettings.Username;
                TokenKey = bbSettings.PublicKey;

                Repositories = new ObservableCollection<RepositoriesViewDto>();
                List<Octokit.Repository> repos = GithubService.GetAllRepositories();
                if (repos != null)
                {
                    foreach (Octokit.Repository repo in repos)
                    {
                        Repositories.Add(new RepositoriesViewDto()
                        {
                            RepoName = repo.Name,
                            Owner = repo.Owner.Login,
                            PicUrl = repo.Owner.AvatarUrl,
                        });
                    }
                }
            }
        }

        public override void OnApplySettingChanges()
        {
            SetCredentials(GithubBackupService.SERVICE_ID, new GithubSettings(GithubBackupService.SERVICE_ID)
            {
                Username = Username,
                PublicKey = TokenKey,
            });
        }
    }
}

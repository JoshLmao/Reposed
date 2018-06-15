using Caliburn.Micro;
using Reposed.Core;
using Reposed.Core.Services.Github;
using Reposed.MVVM;
using Reposed.ServiceComponents.Shared.Models;
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
                TokenKey = GithubService.Token;

                Repositories = new ObservableCollection<RepositoriesViewDto>();
                var repos = GithubService.GetAllRepositories();
                if (repos != null)
                {
                    foreach (Octokit.Repository repo in repos)
                    {
                        Repositories.Add(new RepositoriesViewDto() { RepoName = repo.Name });
                    }
                }
            }
        }

        public void OnRepoCheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(Repositories.Select(x => new Models.BackupReposDto()
            {
                RepositoryName = x.RepoName,
                ShouldBackup = x.ShouldBackup,
            }).ToList());
        }

        public void OnRepoUncheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(Repositories.Select(x => new Models.BackupReposDto()
            {
                RepositoryName = x.RepoName,
                ShouldBackup = x.ShouldBackup,
            }).ToList());
        }
    }
}

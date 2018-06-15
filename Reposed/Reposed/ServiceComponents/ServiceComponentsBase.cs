using Caliburn.Micro;
using Reposed.Core;
using Reposed.Events;
using Reposed.MVVM;
using Reposed.ServiceComponents.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents
{
    public class ServiceComponentsBase : ViewModelBase, IServiceComponent,
        IHandle<PreferencesUpdated>
    {
        protected IBackupService m_service;

        readonly IEventAggregator EVENT_AGGREGATOR = null;

        ObservableCollection<RepositoriesViewDto> m_repositories;
        public ObservableCollection<RepositoriesViewDto> Repositories
        {
            get { return m_repositories; }
            set
            {
                m_repositories = value;
                NotifyOfPropertyChange(() => Repositories);
            }
        }

        public ServiceComponentsBase(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public virtual void Handle(PreferencesUpdated message)
        {
        }

        public virtual void OnRepoCheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(Repositories.Select(x => new Models.BackupReposDto()
            {
                RepositoryName = x.RepoName,
                ShouldBackup = x.ShouldBackup,
            }).ToList());
        }

        public virtual void OnRepoUncheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(Repositories.Select(x => new Models.BackupReposDto()
            {
                RepositoryName = x.RepoName,
                ShouldBackup = x.ShouldBackup,
            }).ToList());
        }
    }
}

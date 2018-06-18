using Caliburn.Micro;
using Reposed.Core;
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

        protected readonly BackupSettingsService BACKUP_SETTINGS_SERVICE = null;

        protected IBackupSettings m_serviceSettings = null;
        protected bool m_hasInitialized = false;

        public ServiceComponentsBase(IEventAggregator eventAggregator, BackupSettingsService settingsService)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);

            BACKUP_SETTINGS_SERVICE = settingsService;
        }

        private void OnServiceAuthorizationChanged(bool isAuthorized)
        {
            UpdateUI();
        }

        public virtual void Handle(PreferencesUpdated message)
        {
        }

        public virtual void OnViewLoaded()
        {
            if (!m_hasInitialized)
            {
                UpdateUI();
                m_hasInitialized = true;
            }
        }

        public virtual void OnRepoCheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(ConvertUi());
        }

        public virtual void OnRepoUncheckedChanged(ActionExecutionContext e)
        {
            m_service.SetBackupRepos(ConvertUi());
        }

        List<BackupReposDto> ConvertUi()
        {
            return Repositories?.Select(x => new BackupReposDto()
            {
                RepositoryName = x.RepoName,
                ShouldBackup = x.ShouldBackup,
            }).ToList();
        }

        protected virtual void UpdateUI()
        {
        }

        protected void GetService<T>() where T : IBackupService
        {
            m_service = IoC.GetAll<IBackupService>().FirstOrDefault(x => x is T);
            m_service.OnIsAuthorizedChanged += OnServiceAuthorizationChanged;

            m_service.SetBackupRepos(ConvertUi());
        }

        public virtual void OnApplySettingChanges()
        {
            
        }

        public void SetCredentials(string serviceId, IBackupSettings serviceSettings)
        {
            BACKUP_SETTINGS_SERVICE.Set(serviceId, serviceSettings);
            m_serviceSettings = serviceSettings;
            m_service.SetCredentials(serviceSettings);
            UpdateUI();
        }
    }
}

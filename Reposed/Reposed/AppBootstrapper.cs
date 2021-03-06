﻿using Caliburn.Micro;
using NLog;
using Reposed.Core;
using Reposed.Core.Dialogs;
using Reposed.Core.Services.Bitbucket;
using Reposed.Core.Services.Github;
using Reposed.Core.Services.Gitlab;
using Reposed.Events;
using Reposed.Services;
using Reposed.Services.Plugins;
using Reposed.Shell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Reposed
{
    public class AppBootstrapper : BootstrapperBase
    {
        readonly Logger LOGGER = null;

        SimpleContainer m_iocContainer;
        bool m_hasLoggedExceptionInfo = false;
        DateTime m_startTime = DateTime.MinValue;

        public AppBootstrapper()
        {
            LOGGER = NLog.LogManager.GetCurrentClassLogger();
            LOGGER.Info("Application Started");

            m_startTime = DateTime.Now;

            System.Windows.Application.Current.DispatcherUnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskUnobserveredTaskException;

            Setup();
        }

        void Setup()
        {
            Initialize();
        }

        void OnTaskUnobserveredTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LOGGER.Fatal(e.Exception);
            OnProgramUnexpectedClose(e.Exception);
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LOGGER.Fatal(e.ExceptionObject);
            OnProgramUnexpectedClose(e.ExceptionObject as Exception);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
            LOGGER.Fatal(e.Exception);

            OnProgramUnexpectedClose(e.Exception);
        }

        void OnProgramUnexpectedClose(Exception e = null)
        {
            if (!m_hasLoggedExceptionInfo)
            {
                m_iocContainer.GetInstance<IEventAggregator>().PublishOnCurrentThread(new OnApplicationClosing(true)
                {
                    Exception = e,
                });
                m_hasLoggedExceptionInfo = true;
            }
        }

        protected override void PrepareApplication()
        {
            //ToDo: splash screen

            base.PrepareApplication();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

            LOGGER.Info($"Reposed closed after {DateTime.Now - m_startTime}");
        }

        protected override void Configure()
        {
            m_iocContainer = new SimpleContainer();

            //Add Caliburn helpers
            m_iocContainer.Singleton<IWindowManager, WindowManager>();
            m_iocContainer.Singleton<IEventAggregator, EventAggregator>();

            //Services
            m_iocContainer.Singleton<IBackupService, BackupInformation.BackupInformationService>();
            m_iocContainer.Singleton<IBackupService, BitbucketBackupService>();
            m_iocContainer.Singleton<IBackupService, GithubBackupService>();
            //m_iocContainer.Singleton<IBackupService, GitlabBackupService>();

            m_iocContainer.Singleton<ScheduledBackupService>();
            m_iocContainer.Singleton<BackupSettingsService>();

            //Plugin Services
            m_iocContainer.Singleton<SlackService>();

            //UI Services
            m_iocContainer.Singleton<IMessageBoxService, MessageBoxService>();

            //ViewModels
            m_iocContainer.PerRequest<ShellViewModel>();
            m_iocContainer.Singleton<Menu.MenuViewModel>();
            m_iocContainer.Singleton<Preferences.PreferencesViewModel>();
            m_iocContainer.Singleton<BackupController.BackupControllerViewModel>();
            m_iocContainer.Singleton<Accounts.AccountsViewModel>();
            m_iocContainer.Singleton<ServiceComponents.ServiceComponentsHolderViewModel>();
            m_iocContainer.Singleton<Dialogs.ScheduledBackup.ScheduledBackupViewModel>();

            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, ServiceComponents.Bitbucket.BitbucketBackupComponentViewModel>(BitbucketBackupService.SERVICE_ID);
            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, ServiceComponents.Github.GithubBackupComponentViewModel>(GithubBackupService.SERVICE_ID);
            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, ServiceComponents.Gitlab.GitlabBackupComponentViewModel>(GitlabBackupService.SERVICE_ID);
            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, BackupInformation.BackupInformationViewModel>(BackupInformation.BackupInformationService.SERVICE_ID);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //Load prefs before Shell displayed
            m_iocContainer.GetInstance<BackupSettingsService>().LoadFromFile();

            //Display Shell
            DisplayRootViewFor<ShellViewModel>();

            m_iocContainer.GetInstance<Preferences.PreferencesViewModel>().LoadPreferences();
        }

        protected override object GetInstance(Type service, string key)
        {
            object instance = m_iocContainer.GetInstance(service, key);
            if (instance == null)
                throw new InvalidOperationException("Unable to find instance");
            else
                return instance;
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return m_iocContainer.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            base.BuildUp(instance);
            m_iocContainer.BuildUp(instance);
        }
    }
}

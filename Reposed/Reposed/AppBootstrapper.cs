using Caliburn.Micro;
using NLog;
using Reposed.Core;
using Reposed.Core.Services.Bitbucket;
using Reposed.Core.Services.Github;
using Reposed.ServiceComponents.Bitbucket;
using Reposed.Shell;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Reposed
{
    public class AppBootstrapper : BootstrapperBase
    {
        readonly Logger LOGGER = null;

        SimpleContainer m_iocContainer;

        public AppBootstrapper()
        {
            LOGGER = NLog.LogManager.GetCurrentClassLogger();
            LOGGER.Info("Application Started");

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
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LOGGER.Fatal(e.ExceptionObject);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
            LOGGER.Fatal(e.Exception);
        }

        protected override void PrepareApplication()
        {
            //ToDo: splash screen

            base.PrepareApplication();
        }

        protected override void Configure()
        {
            m_iocContainer = new SimpleContainer();

            //Add Caliburn helpers
            m_iocContainer.Singleton<IWindowManager, WindowManager>();
            m_iocContainer.Singleton<IEventAggregator, EventAggregator>();

            //ViewModels
            m_iocContainer.PerRequest<ShellViewModel>();
            m_iocContainer.Singleton<Menu.MenuViewModel>();
            m_iocContainer.Singleton<Preferences.PreferencesViewModel>();
            m_iocContainer.Singleton<BackupController.BackupControllerViewModel>();
            m_iocContainer.Singleton<Accounts.AccountsViewModel>();
            m_iocContainer.Singleton<ServiceComponents.ServiceComponentsHolderViewModel>();

            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, ServiceComponents.Bitbucket.BitbucketBackupComponentViewModel>(BitbucketBackupService.SERVICE_ID);
            m_iocContainer.Singleton<ServiceComponents.IServiceComponent, ServiceComponents.Github.GithubBackupComponentViewModel>(GithubBackupService.SERVICE_ID);

            //Services
            m_iocContainer.Singleton<IBackupService, BitbucketBackupService>();
            m_iocContainer.Singleton<IBackupService, GithubBackupService>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //Display Shell
            DisplayRootViewFor<ShellViewModel>();

            //Load prefs after IoC init
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

using System;
using Caliburn.Micro;
using Reposed.MVVM;
using Reposed.Services.Plugins;

namespace Reposed.Menu
{
    public class MenuViewModel : ViewModelBase
    {
        bool m_isSlackBotActive;
        public bool IsSlackBotActive
        {
            get { return m_isSlackBotActive; }
            set
            {
                m_isSlackBotActive = value;
                NotifyOfPropertyChange(() => IsSlackBotActive);
            }
        }

        readonly IWindowManager WINDOW_MANAGER;
        readonly SlackService SLACK_BOT_SERVICE;

        public MenuViewModel(IWindowManager windowManager, SlackService slackService)
        {
            WINDOW_MANAGER = windowManager;
            SLACK_BOT_SERVICE = slackService;
            SLACK_BOT_SERVICE.OnBotConnectionChanged += OnSlackBotStatusChanged;
        }

        private void OnSlackBotStatusChanged(bool isConnected)
        {
            IsSlackBotActive = isConnected && SLACK_BOT_SERVICE.IsEnabled;
        }

        public void OnOpenPreferences()
        {
            WINDOW_MANAGER.ShowDialog(IoC.Get<Preferences.PreferencesViewModel>());
        }

        public void OnCloseApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}

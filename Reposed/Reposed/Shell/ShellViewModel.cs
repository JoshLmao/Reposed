using Caliburn.Micro;
using Reposed.Accounts;
using Reposed.BackupController;
using Reposed.Core.Dialogs;
using Reposed.Events;
using Reposed.Menu;
using Reposed.MVVM;
using Reposed.ServiceComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Shell
{
    public class ShellViewModel : Conductor<Screen>
    {
        public override string DisplayName { get { return "Reposed"; } set { } }

        public MenuViewModel MenuViewModel { get; set; }
        public AccountsViewModel AccountsViewModel { get; set; }
        public BackupControllerViewModel BackupControllerViewModel { get; set; }
        public ServiceComponentsHolderViewModel ServiceComponentsHolderViewModel { get; set; }

        readonly IWindowManager WINDOW_MANAGER = null;
        readonly IEventAggregator EVENT_AGGREAGATOR = null;
        readonly IMessageBoxService MSG_BOX_SERVICE = null;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, IMessageBoxService msgBoxService, MenuViewModel menuViewModel, AccountsViewModel accountsViewModel, BackupControllerViewModel backupControllerViewModel,
            ServiceComponentsHolderViewModel serviceComponentsViewModel)
        {
            WINDOW_MANAGER = windowManager;
            EVENT_AGGREAGATOR = eventAggregator;
            MSG_BOX_SERVICE = msgBoxService;

            MenuViewModel = menuViewModel;
            MenuViewModel.ConductWith(this);

            AccountsViewModel = accountsViewModel;
            AccountsViewModel.ConductWith(this);

            BackupControllerViewModel = backupControllerViewModel;
            BackupControllerViewModel.ConductWith(this);

            ServiceComponentsHolderViewModel = serviceComponentsViewModel;
            ServiceComponentsHolderViewModel.ConductWith(this);
        }

        public void OnAppClosing(ActionExecutionContext e)
        {
            if (CanCloseApp())
            {
                EVENT_AGGREAGATOR.PublishOnCurrentThread(new OnApplicationClosing(false));
            }
            else
            {
                (e.EventArgs as System.ComponentModel.CancelEventArgs).Cancel = true;
            }
        }

        public bool CanCloseApp()
        {
            if (BackupControllerViewModel.IsBackingUp)
            {
                System.Windows.MessageBoxResult rst = MSG_BOX_SERVICE.Show("Are you sure you want to close? A backup is currently in progress", "Backup in progress...", System.Windows.MessageBoxButton.YesNo);
                if (rst == System.Windows.MessageBoxResult.No)
                    return false;
            }

            if (BackupControllerViewModel.IsScheduledEnabled)
            {
                System.Windows.MessageBoxResult rst = MSG_BOX_SERVICE.Show("Are you sure you want to close? Scheduled backup is enabled and will be cancelled", "Scheduled Backup enabled", System.Windows.MessageBoxButton.YesNo);
                if (rst == System.Windows.MessageBoxResult.No)
                    return false;
            }

            return true;
        }
    }
}

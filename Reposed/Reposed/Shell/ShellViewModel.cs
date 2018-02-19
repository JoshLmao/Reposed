using Caliburn.Micro;
using Reposed.Accounts;
using Reposed.BackupController;
using Reposed.Menu;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Shell
{
    public class ShellViewModel : Conductor<IViewModel>
    {
        public override string DisplayName { get { return "Reposed"; } set { } }

        public MenuViewModel MenuViewModel { get; set; }
        public AccountsViewModel AccountsViewModel { get; set; }
        public BackupControllerViewModel BackupControllerViewModel { get; set; }

        readonly IWindowManager WINDOW_MANAGER = null;
        readonly IEventAggregator EVENT_AGGREAGATOR = null;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, MenuViewModel menuViewModel, AccountsViewModel accountsViewModel, BackupControllerViewModel backupControllerViewModel)
        {
            WINDOW_MANAGER = windowManager;
            EVENT_AGGREAGATOR = eventAggregator;

            MenuViewModel = menuViewModel;
            MenuViewModel.ConductWith(this);

            AccountsViewModel = accountsViewModel;
            AccountsViewModel.ConductWith(this);

            BackupControllerViewModel = backupControllerViewModel;
            BackupControllerViewModel.ConductWith(this);
        }
    }
}

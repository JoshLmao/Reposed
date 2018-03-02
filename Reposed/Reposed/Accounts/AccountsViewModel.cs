using Caliburn.Micro;
using Reposed.Core;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Accounts
{
    public class AccountsViewModel : ViewModelBase
    {
        ObservableCollection<IBackupService> m_backupAccounts = new ObservableCollection<IBackupService>();
        public ObservableCollection<IBackupService> BackupAccounts
        {
            get { return m_backupAccounts; }
            set
            {
                m_backupAccounts = value;
                NotifyOfPropertyChange(() => BackupAccounts);
            }
        }

        IBackupService m_selectedAccount;
        public IBackupService SelectedAccount
        {
            get { return m_selectedAccount; }
            set
            {
                m_selectedAccount = value;
                NotifyOfPropertyChange(() => SelectedAccount);
            }
        }

        public AccountsViewModel()
        {

        }

        public override void OnViewLoaded(ActionExecutionContext e)
        {
            UpdateAccounts();
        }

        void UpdateAccounts()
        {
            BackupAccounts.Clear();

            List<IBackupService> services = IoC.GetAll<IBackupService>().ToList();
            foreach (IBackupService service in services)
            {
                if (service.IsAuthorized)
                {
                    BackupAccounts.Add(service);
                }
            }

            if (SelectedAccount == null)
                SelectedAccount = BackupAccounts.FirstOrDefault();

            NotifyOfPropertyChange(() => BackupAccounts);
        }
    }
}

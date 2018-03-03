using Reposed.Core;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents
{
    public class ServiceComponentsBase : ViewModelBase, IServiceComponent
    {
        protected IBackupService m_service;

        public ServiceComponentsBase()
        {

        }
    }
}

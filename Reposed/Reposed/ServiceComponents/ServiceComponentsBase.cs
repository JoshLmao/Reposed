using Caliburn.Micro;
using Reposed.Core;
using Reposed.Events;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
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

        public ServiceComponentsBase(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public virtual void Handle(PreferencesUpdated message)
        {
        }
    }
}

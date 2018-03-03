using Caliburn.Micro;
using Reposed.Core;
using Reposed.Events;
using Reposed.ServiceComponents.Bitbucket;
using Reposed.ServiceComponents.Github;
using System.Collections.Generic;

namespace Reposed.ServiceComponents
{
    public class ServiceComponentsHolderViewModel : Conductor<IServiceComponent>, MVVM.IViewModel, IHandle<OnAccountSelected>
    {
        IBackupService m_selectedBackupService;

        readonly IEventAggregator EVENT_AGGREGATOR = null;
        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ServiceComponentsHolderViewModel(IEventAggregator eventAggregator)
        {
            EVENT_AGGREGATOR = eventAggregator;
            EVENT_AGGREGATOR.Subscribe(this);
        }

        public void Handle(OnAccountSelected message)
        {
            m_selectedBackupService = message.Service;
            SetView(m_selectedBackupService.ServiceId);
        }

        void SetView(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return;

            if (ActiveItem != null)
                DeactivateItem(ActiveItem, true);

            IServiceComponent component = IoC.Get<IServiceComponent>(serviceId) as IServiceComponent;
            ActivateItem(component);
        }
    }
}

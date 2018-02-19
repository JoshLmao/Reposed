using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.MVVM
{
    public class ViewModelBase : Screen, IViewModel
    {
        protected readonly Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ViewModelBase()
        {

        }

        public virtual void OnViewLoaded(ActionExecutionContext e)
        {

        }
    }
}

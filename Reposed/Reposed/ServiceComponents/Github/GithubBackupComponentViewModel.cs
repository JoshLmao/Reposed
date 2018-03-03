﻿using Caliburn.Micro;
using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Github
{
    public class GithubBackupComponentViewModel : ServiceComponentsBase
    {
        public GithubBackupComponentViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
﻿using Reposed.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Events
{
    public class OnAccountSelected
    {
        public IBackupService Service { get; set; }
        public OnAccountSelected(IBackupService service)
        {
            Service = service;
        }
    }
}

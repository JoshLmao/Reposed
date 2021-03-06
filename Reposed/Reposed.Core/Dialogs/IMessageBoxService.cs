﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reposed.Core.Dialogs
{
    public interface IMessageBoxService
    {
        MessageBoxResult Show(string message);
        MessageBoxResult Show(string message, string title);
        MessageBoxResult Show(string message, string title, MessageBoxButton btns);
    }
}

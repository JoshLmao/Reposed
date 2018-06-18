using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reposed.Core.Dialogs
{
    public class MessageBoxService : IMessageBoxService
    {
        public MessageBoxResult Show(string message)
        {
            return MessageBox.Show(message);
        }

        public MessageBoxResult Show(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        public MessageBoxResult Show(string message, string title, MessageBoxButton btns)
        {
            return MessageBox.Show(message, title, btns);
        }
    }
}

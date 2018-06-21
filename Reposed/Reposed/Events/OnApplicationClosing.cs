using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Events
{
    public class OnApplicationClosing
    {
        public Exception Exception { get; set; }
        public bool IsUnexpected { get; set; }
        public OnApplicationClosing(bool isUnexpectedShutdown)
        {
            IsUnexpected = isUnexpectedShutdown;
        }
    }
}

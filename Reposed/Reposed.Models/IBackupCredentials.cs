using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Models
{
    public interface IBackupSettings
    {
        string ServiceId { get; set; }

        string Username { get; set; }
        string Password { get; set; }

        string PublicKey { get; set; }
    }
}

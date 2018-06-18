using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Models
{
    public class BitBucketSettings : IBackupSettings
    {
        public string ServiceId { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public BitBucketSettings(string serviceId)
        {
            ServiceId = serviceId;
        }
    }
}

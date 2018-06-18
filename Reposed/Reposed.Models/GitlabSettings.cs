using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Models
{
    public class GitlabSettings : IBackupCredentials
    {
        public string Username { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public string Password { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}

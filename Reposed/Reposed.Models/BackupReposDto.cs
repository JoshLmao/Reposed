using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Models
{
    public class BackupReposDto
    {
        public string RepositoryName { get; set; }
        public bool ShouldBackup { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Models
{
    public class Preferences
    {
        public string LocalGitPath { get; set; }
        public string LocalBackupPath { get; set; }

        public BitBucketSettings BitbucketPrefs { get; set; }
        public GithubSettings GithubPrefs { get; set; }
        public GitlabSettings GitLabSettings { get; set; }
    }
}

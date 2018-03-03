﻿using System;
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

        public BitBucketPrefs BitbucketPrefs { get; set; }
        public GithubPrefs GithubPrefs { get; set; }
    }
}

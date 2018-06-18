using System;
using System.Collections.Generic;

namespace Reposed.Core.Services.Gitlab
{
    public class GitlabAPIService
    {
        public GitlabAPIService(string username, string publicKey, string privateKey)
        {
        }

        public List<object> GetAllRepositories()
        {
            return null;
        }

        public bool IsAuthorized()
        {
            return false;
        }
    }
}

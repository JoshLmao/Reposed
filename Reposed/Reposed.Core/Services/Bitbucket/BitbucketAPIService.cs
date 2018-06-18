using SharpBucket.V2;
using SharpBucket.V2.EndPoints;
using SharpBucket.V2.Pocos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Bitbucket
{
    public class BitbucketAPIService
    {
        public string Username { get; private set; }

        SharpBucketV2 m_sharpBucket2 = null;

        public BitbucketAPIService(string username, string publicKey, string secretKey)
        {
            Username = username;
            
            m_sharpBucket2 = new SharpBucketV2();
            m_sharpBucket2.OAuth2LeggedAuthentication(publicKey, secretKey);
        }

        public List<Repository> GetAllRepos(string usernameOrTeamName)
        {
            RepositoriesEndPoint repoEndPoint = m_sharpBucket2.RepositoriesEndPoint();
            List<Repository> allRepos = null;
            if (!string.IsNullOrEmpty(usernameOrTeamName))
            {
                allRepos = repoEndPoint.ListRepositories(usernameOrTeamName);
            }

            return allRepos;
        }

        public string GetRepoUrl(string repo, bool isSSH)
        {
            RepositoriesEndPoint reposEndPoint = m_sharpBucket2.RepositoriesEndPoint();
            List<Repository> repos = reposEndPoint.ListRepositories(Username);
            Repository foundRepo = repos.FirstOrDefault(x => x.name == repo);
            if (foundRepo != null)
                if (isSSH)
                    return foundRepo.links.clone[1].href;
                else
                    return foundRepo.links.clone[0].href;
            else
                return null;
        }

        public bool IsAuthorized()
        {
            return m_sharpBucket2?.UserEndPoint().GetUser() != null;
        }
    }
}

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
        string m_teamName;
        string m_username;

        SharpBucketV2 m_sharpBucket2 = null;

        public BitbucketAPIService(string publicKey, string secretKey)
        {
            m_sharpBucket2 = new SharpBucketV2();
            m_sharpBucket2.OAuth2LeggedAuthentication(publicKey, secretKey);
        }

        public void GetAllRepos(string usernameOrTeamName)
        {
            RepositoriesEndPoint repoEndPoint = m_sharpBucket2.RepositoriesEndPoint();
            List<Repository> allRepos = null;
            if (!string.IsNullOrEmpty(usernameOrTeamName))
            {
                allRepos = repoEndPoint.ListRepositories(usernameOrTeamName);
            }

            return;
        }
    }
}

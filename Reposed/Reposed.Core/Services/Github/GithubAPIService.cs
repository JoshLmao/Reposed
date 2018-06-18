using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Services.Github
{
    public class GithubAPIService
    {
        public string Username { get; private set; }
        public string Token { get; private set; }

        GitHubClient m_client = null;

        const string PRODUCT_NAME = "Reposed";

        public GithubAPIService(string username, string token)
        {
            Username = username;
            Token = token;

            if (!string.IsNullOrEmpty(token))
            {
                m_client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));
                Credentials tokenAuth = new Credentials(Token);
                m_client.Credentials = tokenAuth;
            }
        }

        public List<Repository> GetAllRepositories()
        {
            if (IsAuthorized())
                return new List<Repository>(m_client.Repository.GetAllForCurrent()?.Result);
            else
                return null;
        }

        public string GetRepoUrl(string repoName)
        {
            List<Repository> repos = GetAllRepositories();
            Repository foundRepo = repos.FirstOrDefault(x => x.Name == repoName);
            return foundRepo?.CloneUrl;
        }

        public bool IsAuthorized()
        {
            //Needs to be a better way than this
            try
            {
                var r = m_client.Repository.GetAllForCurrent()?.Result;
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
    }
}

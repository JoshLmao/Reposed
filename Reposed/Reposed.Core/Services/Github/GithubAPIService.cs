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
        GitHubClient m_client = null;

        public GithubAPIService(string username)
        {
            m_client = new GitHubClient(new ProductHeaderValue(username));
            User user = m_client.User.Get(username).Result;
            Console.WriteLine(user.Login);
        }

        public List<Repository> GetAllRepositories()
        {
            return new List<Repository>(m_client.Repository.GetAllForCurrent().Result);
        }
    }
}

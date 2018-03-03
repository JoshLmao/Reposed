using Reposed.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.ServiceComponents.Bitbucket
{
    public class BitbucketBackupComponentViewModel : ServiceComponentsBase
    {
        public override string DisplayName { get { return "Bitbucket Backup View"; } set { } }

        public string m_username;
        public string Username
        {
            get { return m_username; }
            set
            {
                m_username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        string m_oathPublic;
        public string OAuthPublic
        {
            get { return m_oathPublic; }
            set
            {
                m_oathPublic = value;
                NotifyOfPropertyChange(() => OAuthPublic);
            }
        }

        string m_oauthPrivate;
        public string OAuthPrivate
        {
            get { return m_oauthPrivate; }
            set
            {
                m_oauthPrivate = value;
                NotifyOfPropertyChange(() => OAuthPrivate);
            }
        }

        public BitbucketBackupComponentViewModel()
        {
            Username = "TestName";
        }

        public void OnViewLoaded()
        {

        }
    }
}

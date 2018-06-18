using Newtonsoft.Json.Linq;
using Reposed.Core.Services.Bitbucket;
using Reposed.Core.Services.Github;
using Reposed.Core.Services.Gitlab;
using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Core.Json
{
    public class BackupSettingsConverter : CustomConverterBase<IBackupSettings>
    {
        protected override IBackupSettings Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ServiceId", jObject))
            {
                string field = (string)jObject["ServiceId"];

                if (field == BitbucketBackupService.SERVICE_ID)
                {
                    return new BitBucketSettings(BitbucketBackupService.SERVICE_ID);
                }
                else if (field == GithubBackupService.SERVICE_ID)
                {
                    return new GithubSettings(GithubBackupService.SERVICE_ID);
                }
                else if(field == GitlabBackupService.SERVICE_ID)
                {
                    return new GitlabSettings(GitlabBackupService.SERVICE_ID);
                }
                else
                {
                    throw new NotImplementedException($"Implement missing Backup Settings model - Missing '{field}'");
                }
            }

            throw new NullReferenceException($"The field 'Name' is missing from jObject {jObject} in {this.GetType().Name}");
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject["ServiceId"] != null;
        }
    }
}

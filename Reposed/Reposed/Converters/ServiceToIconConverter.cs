using Reposed.Core;
using Reposed.Core.Services.Bitbucket;
using Reposed.Core.Services.Github;
using Reposed.Core.Services.Gitlab;
using Reposed.ServiceComponents.BackupInformation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Reposed.Converters
{
    internal class ServiceToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IBackupService service)
            {
                if (value is BitbucketBackupService)
                    return Application.Current.FindResource("BitbucketIcon");
                else if (value is GithubBackupService)
                    return Application.Current.FindResource("GitHubIcon");
                else if (value is GitlabBackupService)
                    return Application.Current.FindResource("GitLabIcon");
                else if (value is BackupInformationService)
                    return Application.Current.FindResource("BackupInformationIcon");
                else
                    throw new NotImplementedException("Add icon!!");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

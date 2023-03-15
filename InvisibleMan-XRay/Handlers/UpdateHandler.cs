using System;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace InvisibleManXRay.Handlers
{
    using Models;
    using Values;

    public class UpdateHandler : Handler
    {
        public Status CheckForUpdate()
        {
            string latestReleaseUrl = GetLatestReleaseUrl();
            string latestReleaseVersion = GetLatestReleaseVersion(latestReleaseUrl);
            if (latestReleaseVersion == null)
                return new Status(Code.ERROR, SubCode.CANT_CONNECT, Message.CANT_CONNECT_TO_SERVER);

            if (IsUpdateAvailable())
                return new Status(Code.SUCCESS, SubCode.UPDATE_AVAILABLE, Message.UPDATE_AVAILABLE);
            
            return new Status(Code.SUCCESS, SubCode.UPDATE_UNAVAILABLE, Message.YOU_HAVE_LATEST_VERSION);

            bool IsUpdateAvailable()
            {
                AppVersion latestReleaseAppVersion = ConvertToAppVersion(latestReleaseVersion);
                AppVersion currentReleaseAppVersion = ConvertToAppVersion(GetCurrentReleaseVersion());
                
                if (latestReleaseAppVersion.Major > currentReleaseAppVersion.Major)
                    return true;
                else if (latestReleaseAppVersion.Major < currentReleaseAppVersion.Major)
                    return false;
                
                if (latestReleaseAppVersion.Feature > currentReleaseAppVersion.Feature)
                    return true;
                else if (latestReleaseAppVersion.Feature < currentReleaseAppVersion.Feature)
                    return false;
                
                if (latestReleaseAppVersion.BugFix > currentReleaseAppVersion.BugFix)
                    return true;
                else if (latestReleaseAppVersion.BugFix < currentReleaseAppVersion.BugFix)
                    return false;
                
                return false;
            }

            AppVersion ConvertToAppVersion(string version)
            {
                string[] versionElements = version.Split('.');
                
                return new AppVersion() {
                    Major = versionElements.Length > 0 ? TryConvertStringToInt(versionElements[0]) : 0,
                    Feature = versionElements.Length > 1 ? TryConvertStringToInt(versionElements[1]) : 0,
                    BugFix = versionElements.Length > 2 ? TryConvertStringToInt(versionElements[2]) : 0
                };

                int TryConvertStringToInt(string str)
                {
                    try
                    {
                        return int.Parse(str);
                    }
                    catch(Exception)
                    {
                        return 0;
                    }
                }
            }
        }

        public void OpenUpdateWebPage()
        {
            Process.Start(new ProcessStartInfo(Route.LATEST_RELEASE) {
                UseShellExecute = true
            });
        }

        private string GetLatestReleaseUrl()
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(Route.LATEST_RELEASE) as HttpWebRequest;
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                
                return response.Headers["Location"];
            }
            catch(Exception)
            {
                return null;
            }
        }

        private string GetLatestReleaseVersion(string latestReleaseUrl)
        {
            return latestReleaseUrl == null ? null : latestReleaseUrl.Split("/").Last();
        }

        private string GetCurrentReleaseVersion() 
        {
            Version version = GetType().Assembly.GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
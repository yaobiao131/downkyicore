using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DownKyi.Core.Settings;
using DownKyi.Models;
using Newtonsoft.Json;

namespace DownKyi.Services
{
    public class VersionCheckerService
    {
        private readonly string _repoOwner;  
        private readonly string _repoName;  
        private readonly bool _includePrereleases;
        
        public VersionCheckerService(string repoOwner, string repoName, bool includePrereleases = false)
        {
            _repoOwner = repoOwner;
            _repoName = repoName;
            _includePrereleases = includePrereleases;
        }

     
        public async Task<GitHubRelease?> GetLatestReleaseAsync(string? ignoreVersion = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "downkyi");
            client.Timeout = TimeSpan.FromSeconds(3);
            try
            {
#pragma warning disable IL2026
                if (_includePrereleases)
                {
                    var releasesUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases";
                    var releasesJson = await client.GetStringAsync(releasesUrl);

                    var releases = JsonConvert.DeserializeObject<GitHubRelease[]>(releasesJson);
                    if (string.IsNullOrEmpty(ignoreVersion))
                    {
                        return releases?.First();
                    }
                    else
                    {
                        if (releases != null && 
                            releases.Any() && 
                            (releases.First().TagName.TrimStart('v') != ignoreVersion))
                        {
                            return releases.First();
                        }
                    }
                
                }
                else
                {
                    var latestReleaseUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
                    var latestReleaseJson = await client.GetStringAsync(latestReleaseUrl);
                    var release = JsonConvert.DeserializeObject<GitHubRelease>(latestReleaseJson);

                    if (string.IsNullOrEmpty(ignoreVersion))
                    {
                        return release;
                    }
                    else
                    {
                        if (release != null &&
                            release.TagName.TrimStart('v') != ignoreVersion)
                        {
                            return release;
                        }
                    }
                }
#pragma warning restore IL2026
            }
            catch (Exception e)
            {
               /**/
            }

            return null;
        }

      


        public bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            var current = new Version(currentVersion.TrimStart('v'));
            var latest = new Version(latestVersion.TrimStart('v'));
            return latest > current;
        }
       
    }
}

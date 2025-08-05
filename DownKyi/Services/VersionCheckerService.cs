using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

     
        public async Task<GitHubRelease?> GetLatestReleaseAsync(string? excludedVersion  = null)
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
            
                    return string.IsNullOrEmpty(excludedVersion) 
                        ? releases?.FirstOrDefault() 
                        : releases?.FirstOrDefault(r => r.TagName.TrimStart('v') != excludedVersion);
                }
                else
                {
                    var latestReleaseUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
                    var latestReleaseJson = await client.GetStringAsync(latestReleaseUrl);
                    var release = JsonConvert.DeserializeObject<GitHubRelease>(latestReleaseJson);
            
                    return string.IsNullOrEmpty(excludedVersion) || 
                           release?.TagName.TrimStart('v') != excludedVersion ? release : null;
                }
#pragma warning restore IL2026
            }
            catch (Exception e)
            {
               /**/
            }

            return null;
        }

      


        public bool IsNewVersionAvailable(string latestVersion)
        {
            string v = new AppInfo().VersionName;
#if DEBUG
            v = v.Replace("-debug", string.Empty);
#endif
            var current = new Version(v.TrimStart('v'));
            var latest = new Version(latestVersion.TrimStart('v'));
            return latest > current;
        }
       
    }
}

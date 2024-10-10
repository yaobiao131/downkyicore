using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DownKyi.Services
{
    public class VersionCheckerService
    {
        private const string gh_releases = "https://api.github.com/repos/yaobiao131/downkyicore/releases/latest";
        public async Task<(Version, string)> GetLatestVersion()
        {
            string json;
            try
            {
                var hc = new HttpClient();
                hc.DefaultRequestHeaders.Add("User-Agent", "downkyicore");

                json =await hc.GetStringAsync(new Uri(gh_releases));
                    
            }
            catch (Exception e) when (e is HttpRequestException or TimeoutException)
            {
                return (null, null);
            }
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                var versionString = doc.RootElement.GetProperty("tag_name").GetString();
                var updateNotes = doc.RootElement.GetProperty("body").GetString();
                var version = Version.Parse(versionString.TrimStart('v'));
                return (version, updateNotes);
            }
            catch (Exception e)
            {
                return (null, null);
            }
        }
    }
}

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DownKyi.Services
{
    public class VersionCheckerService
    {
        private const string GhReleases = "https://api.github.com/repos/yaobiao131/downkyicore/releases/latest";
        public async Task<(Version?, string?)> GetLatestVersion()
        {
            Version? version = default;
            string? updateNotes = default;
            try
            {
                using var hc = new HttpClient();
                hc.DefaultRequestHeaders.Add("User-Agent", "downkyicore");
                var json =await hc.GetStringAsync(new Uri(GhReleases));
                using var doc = JsonDocument.Parse(json);
                var versionString = doc.RootElement.GetProperty("tag_name").GetString()!;
                updateNotes = doc.RootElement.GetProperty("body").GetString()!;
                version = Version.Parse(versionString.TrimStart('v'));
                return (version, updateNotes);
            }
            catch (Exception e)
            {
                return (null, null);
            }
          
        }
    }
}

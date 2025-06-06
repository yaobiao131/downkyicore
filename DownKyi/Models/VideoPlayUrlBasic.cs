using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownKyi.Models
{
    public class VideoPlayUrlBasic
    {
        public List<string> BackupUrl { get; set; }
        public string BaseUrl { get; set; }

        public int Id { get; set; }

        public string Codecs { get; set; }
    }
}

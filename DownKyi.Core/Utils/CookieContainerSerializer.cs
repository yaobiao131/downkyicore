using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DownKyi.Core.Utils
{
    public class CookieContainerSerializer
    {
        private sealed class SerializableCookie
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public DateTime? Expires { get; set; }

        }
        public static string SerializeCookieContainer(CookieContainer container)
        {
            var cookies = ObjectHelper.GetAllCookies(container);
            var serializableCookies = cookies.Select(c => new SerializableCookie
            {
                Name = c.Name,
                Value = c.Value,
                Domain = c.Domain,
                Path = c.Path,
                Expires = c.Expires,
            }).ToList();

            return JsonSerializer.Serialize(serializableCookies);
        }

        public static CookieContainer DeserializeCookieContainer(string json)
        {
            var serializableCookies = JsonSerializer.Deserialize<List<SerializableCookie>>(json);
            var cookieContainer = new CookieContainer();

            foreach (var sc in serializableCookies)
            {
                var cookie = new Cookie(sc.Name, sc.Value, sc.Path, sc.Domain)
                {
                    Expires = sc.Expires ?? DateTime.MinValue
                
                };
                cookieContainer.Add(cookie);
            }

            return cookieContainer;
        }
    }
}

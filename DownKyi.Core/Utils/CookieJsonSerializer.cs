using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DownKyi.Core.Utils;

public static class CookieJsonSerializer
{
    private static readonly JsonSerializerOptions options = new()
    {
        Converters = { new CookieContainerConverter() }
    };
    
    public static string Serialize(CookieContainer container) => 
        JsonSerializer.Serialize(container, options);

    public static CookieContainer Deserialize(string json) => 
        JsonSerializer.Deserialize<CookieContainer>(json, options)!;
    
    private class CookieContainerConverter : JsonConverter<CookieContainer>
    {
        public override CookieContainer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var container = new CookieContainer();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    string name = string.Empty;
                    string value = string.Empty;
                    string domain = string.Empty;
                    string path = "/";
                    DateTime expires = DateTime.MinValue; 
                    
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            string propName = reader.GetString()!;
                            reader.Read();
                            switch (propName.ToLower())
                            {
                                case "name":
                                    name = reader.GetString() ?? "";
                                    break;
                                case "value":
                                    value = reader.GetString() ?? "";
                                    break;
                                case "domain":
                                    domain = reader.GetString() ?? "";
                                    break;
                                case "path":
                                    path = reader.GetString() ?? "/";
                                    break;
                                case "expires":
                                    if (reader.TokenType == JsonTokenType.String)
                                    {
                                        if (DateTime.TryParse(reader.GetString(), out var parsedDate))
                                        {
                                            expires = parsedDate;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    container.Add(new Cookie(name, value, path, domain)
                    {
                        Expires = expires
                    });
                }
                
            }
            return container;
        }

        public override void Write(Utf8JsonWriter writer, CookieContainer value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (Cookie cookie in value.GetAllCookies())
            {
                writer.WritePropertyName(cookie.Name);
                JsonSerializer.Serialize(writer, cookie);
            }
            writer.WriteEndObject();
        }
    }
}
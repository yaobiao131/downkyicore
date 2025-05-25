using System.Net;
using System.Text.Json.Serialization;

namespace DownKyi.Core.Storage;

public class DownKyiCookie
{
    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("value")] public string? Value { get; set; }

    [JsonPropertyName("domain")] public string? Domain { get; set; }

    public DownKyiCookie()
    {
    }

    public DownKyiCookie(string name)
    {
        Name = name;
    }

    public DownKyiCookie(string name, string? value) : this(name)
    {
        Value = value;
    }

    public DownKyiCookie(string name, string? value, string? domain) : this(name, value)
    {
        Domain = domain;
    }

    public Cookie ToSystemNetCookie()
    {
        return new Cookie(Name, Value, "/", Domain);
    }
}
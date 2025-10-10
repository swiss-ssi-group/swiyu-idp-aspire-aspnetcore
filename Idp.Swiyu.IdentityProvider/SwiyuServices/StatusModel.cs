using System.Text.Json.Serialization;

namespace Idp.Swiyu.IdentityProvider.SwiyuServices;

public class StatusModel
{
    [JsonPropertyName("id")]
    public string id { get; set; } = null!;
    [JsonPropertyName("status")]
    public string status { get; set; } = null!;
}
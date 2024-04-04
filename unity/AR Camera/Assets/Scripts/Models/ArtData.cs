using System;
using Newtonsoft.Json;

namespace Models
{
    [Serializable]
    public class ArtData
    {
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("userId")] public string UserId { get; set; }
        [JsonProperty("artId")] public int ArtId { get; set; }
        [JsonProperty("size")] public Size Size { get; set; }
    }
}
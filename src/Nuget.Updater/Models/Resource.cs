namespace Nuget.Updater.Models
{
    using Newtonsoft.Json;

    public class Resource
    {
        [JsonProperty("@id")]
        public string Url { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Comment { get; set; }
    }
}
namespace Nuget.Updater.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Feed
    {
        public string Version { get; set; }
        [JsonProperty("resources")]
        public List<Resource> Resources { get; set; }
    }
}
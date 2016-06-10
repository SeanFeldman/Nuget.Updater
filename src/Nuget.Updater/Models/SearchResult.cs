namespace Nuget.Updater.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SearchResult
    {
        public string Index { get; set; }
        public DateTime LastReopen { get; set; }
        [JsonProperty("data")]
        public List<SearchResultPackage> Data { get; set; }
    }
}
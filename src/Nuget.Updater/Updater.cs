namespace Nuget.Updater
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NuGet;
    using HttpClient = System.Net.Http.HttpClient;

    public class Updater
    {
        Uri nugetV3Uri = new Uri("https://www.myget.org/F/messagehandler-dist/api/v3/index.json");

        public async Task<SemanticVersion> GetLatestVersion(string packageId, bool includePreRelease = false, CancellationToken token = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                var feed = await Get<Feed>(nugetV3Uri, httpClient, token);
                var searchQueryService = feed.Resources.FirstOrDefault(x => x.Type == "SearchQueryService");

                var searchPackageUri = new Uri($"{searchQueryService.Url}/?q=packageid:{packageId}&prerelease={includePreRelease}");
                var searchResult = await Get<SearchResult>(searchPackageUri, httpClient, token);
                var searchResultPackage = searchResult.Data.FirstOrDefault();
                return new SemanticVersion(searchResultPackage.Version);
            }
        }

        async Task<T> Get<T>(Uri uri, HttpClient httpClient, CancellationToken token)
        {
            var response = await httpClient.GetAsync(uri, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }

    public class Feed
    {
        public string Version { get; set; }
        [JsonProperty("resources")]
        public List<Resource> Resources { get; set; }
    }

    public class Resource
    {
        [JsonProperty("@id")]
        public string Url { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Comment { get; set; }
    }

    public class SearchResult
    {
        public string Index { get; set; }
        public DateTime LastReopen { get; set; }
        [JsonProperty("data")]
        public List<SearchResultPackage> Data { get; set; }
    }

    public class SearchResultPackage
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Version { get; set; }
    }
}
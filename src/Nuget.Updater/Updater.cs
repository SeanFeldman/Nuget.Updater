namespace Nuget.Updater.Models
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NuGet;
    using HttpClient = System.Net.Http.HttpClient;

    public class Updater
    {
        readonly Uri nugetV3FeedUri;
        Feed feed;

        public Updater(Uri nugetV3FeedUri = null)
        {
            if (nugetV3FeedUri == null)
            {
                nugetV3FeedUri = new Uri("https://www.myget.org/F/messagehandler-dist/api/v3/index.json");
            }
            this.nugetV3FeedUri = nugetV3FeedUri;
        }

        public async Task<SemanticVersion> GetLatestVersionAsync(string packageId, bool includePreRelease = false, CancellationToken token = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                feed = await GetJson<Feed>(nugetV3FeedUri, httpClient, token);
                var searchQueryService = feed.Resources.FirstOrDefault(x => x.Type == "SearchQueryService");

                var searchPackageUri = new Uri($"{searchQueryService.Url}/?q=packageid:{packageId}&prerelease={includePreRelease}");
                var searchResult = await GetJson<SearchResult>(searchPackageUri, httpClient, token);
                var searchResultPackage = searchResult.Data.FirstOrDefault();
                return new SemanticVersion(searchResultPackage.Version);
            }
        }

        public async Task DownloadAsync(string packageId, SemanticVersion version, string destinationPath, CancellationToken token = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (feed == null)
                {
                    feed = await GetJson<Feed>(nugetV3FeedUri, httpClient, token);
                }
                var packageBaseAddress = feed.Resources.FirstOrDefault(x => x.Type.StartsWith("PackageBaseAddress"));
                var packageBaseAddressUri = new Uri($"{packageBaseAddress.Url}/{packageId.ToLower()}/{version}/{packageId.ToLower()}.{version}.nupkg");

                var response = await httpClient.GetAsync(packageBaseAddressUri, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var fileName = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault().Split('=')[1];

                var path = Path.Combine(destinationPath, fileName);
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
        }

        async Task<T> GetJson<T>(Uri uri, HttpClient httpClient, CancellationToken token)
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
}
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
        Uri nugetV3Uri = new Uri("https://www.myget.org/F/messagehandler-dist/api/v3/index.json");
        Feed feed;


        public async Task<SemanticVersion> GetLatestVersion(string packageId, bool includePreRelease = false, CancellationToken token = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                feed = await GetJson<Feed>(nugetV3Uri, httpClient, token);
                var searchQueryService = feed.Resources.FirstOrDefault(x => x.Type == "SearchQueryService");

                var searchPackageUri = new Uri($"{searchQueryService.Url}/?q=packageid:{packageId}&prerelease={includePreRelease}");
                var searchResult = await GetJson<SearchResult>(searchPackageUri, httpClient, token);
                var searchResultPackage = searchResult.Data.FirstOrDefault();
                return new SemanticVersion(searchResultPackage.Version);
            }
        }

        public async Task Download(string packageId, SemanticVersion version, string destinationPath, CancellationToken token = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (feed == null)
                {
                    feed = await GetJson<Feed>(nugetV3Uri, httpClient, token);
                }
                var packageBaseAddress = feed.Resources.FirstOrDefault(x => x.Type.StartsWith("PackageBaseAddress"));
                var packageBaseAddressUri = new Uri($"{packageBaseAddress.Url}/{packageId.ToLower()}/{version}/{packageId.ToLower()}.{version}.nupkg");

                var response = await httpClient.GetAsync(packageBaseAddressUri, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                Console.WriteLine(response.Content.Headers.ContentDisposition?.FileName);
                var path = Path.Combine(destinationPath, $"{packageId}.{version}.nupkg");
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
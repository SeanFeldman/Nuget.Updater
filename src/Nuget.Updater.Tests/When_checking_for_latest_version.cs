﻿namespace Nuget.Updater.Tests
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using NuGet;
    using NUnit.Framework;

    [TestFixture]
    public class When_checking_for_latest_version
    {
        [Test, Explicit("Latest version can change")]
        public async Task Should_report_the_latest_version()
        {
            var latestVersion = await new Updater().GetLatestVersionAsync("Phoenix");
            Assert.That(latestVersion, Is.EqualTo(new SemanticVersion("0.0.7")));
        }

        [Test]
        public async Task Should_download_the_package()
        {
            var updater = new Updater();
            var packageId = "Phoenix";
            var latestVersion = await updater.GetLatestVersionAsync(packageId);
            var destinationPath = Path.GetTempPath();
            await updater.DownloadAsync(packageId, latestVersion, destinationPath, CancellationToken.None);
            Assert.IsTrue(File.Exists(Path.Combine(destinationPath, $"Phoenix.{latestVersion}.nupkg")));
        }
    }
}
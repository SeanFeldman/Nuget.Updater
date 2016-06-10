namespace Nuget.Updater.Tests
{
    using System.Threading.Tasks;
    using NuGet;
    using NUnit.Framework;

    [TestFixture]
    public class When_checking_for_latest_version
    {
        [Test]
        public async Task Should_get_it()
        {
            var latestVersion = await new Updater().GetLatestVersion("Phoenix");
            Assert.That(latestVersion, Is.EqualTo(new SemanticVersion("0.0.7")));
        }
    }
}
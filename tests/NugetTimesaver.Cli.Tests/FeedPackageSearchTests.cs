using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

/// <summary>Exercises the real `dotnet package search` against nuget.org — no project involved.</summary>
public class FeedPackageSearchTests
{
    [Fact]
    public async Task GetLatestVersionAsync_RealPackage_ReturnsAVersionNewerThanAnOldKnownRelease()
    {
        var latest = await FeedPackageSearch.GetLatestVersionAsync(
            "Newtonsoft.Json", FeedResolver.DefaultFeedUrl, includePrerelease: false);

        Assert.NotNull(latest);
        Assert.True(latest > NuGet.Versioning.NuGetVersion.Parse("13.0.0"));
        Assert.False(latest.IsPrerelease);
    }

    [Fact]
    public async Task GetVersionsAsync_UnknownPackage_ReturnsEmpty()
    {
        var versions = await FeedPackageSearch.GetVersionsAsync(
            "This.Package.Definitely.Does.Not.Exist.Anywhere.Xyz123", FeedResolver.DefaultFeedUrl, includePrerelease: false);

        Assert.Empty(versions);
    }
}

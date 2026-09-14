using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class PackageWildcardMatcherTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NullOrEmptyWildcard_MatchesEverything(string? wildcard)
    {
        Assert.True(PackageWildcardMatcher.IsMatch("IntegrationBroker.Domain", wildcard));
        Assert.True(PackageWildcardMatcher.IsMatch("Newtonsoft.Json", wildcard));
    }

    [Theory]
    [InlineData("IntegrationBroker.Domain", true)]
    [InlineData("integrationbroker.domain", true)]
    [InlineData("IntegrationBroker.Domain.Extra", false)]
    [InlineData("Newtonsoft.Json", false)]
    public void ExactWildcard_MatchesOnlyThatPackage(string packageId, bool expected)
    {
        Assert.Equal(expected, PackageWildcardMatcher.IsMatch(packageId, "IntegrationBroker.Domain"));
    }

    [Theory]
    [InlineData("IntegrationBroker.Services.Models", true)]
    [InlineData("IntegrationBroker.Logging", true)]
    [InlineData("integrationbroker.logging", true)]
    [InlineData("IntegrationBroker", true)]
    [InlineData("Some.IntegrationBroker.Logging", false)]
    [InlineData("Newtonsoft.Json", false)]
    public void PrefixGlob_MatchesExpectedPackages(string packageId, bool expected)
    {
        Assert.Equal(expected, PackageWildcardMatcher.IsMatch(packageId, "IntegrationBroker*"));
    }

    [Theory]
    [InlineData("My.Foo.Bar", true)]
    [InlineData("My.Baz", false)]
    public void MidStringGlob_MatchesAcrossSegments(string packageId, bool expected)
    {
        Assert.Equal(expected, PackageWildcardMatcher.IsMatch(packageId, "My*Bar"));
    }
}

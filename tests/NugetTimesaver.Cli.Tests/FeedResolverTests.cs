using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class FeedResolverTests
{
    // Captured verbatim from `dotnet nuget list source` on a real machine with two private feeds configured.
    private const string SampleOutput = """
        Registered Sources:
          1.  nuget.org [Enabled]
              https://api.nuget.org/v3/index.json
          2.  OTP_PreRelease [Enabled]
              https://pkgs.dev.azure.com/1touchpoint/_packaging/OTP_PreRelease/nuget/v3/index.json
          3.  OTP_Release [Disabled]
              https://pkgs.dev.azure.com/1touchpoint/_packaging/OTP_Release/nuget/v3/index.json
        """;

    [Fact]
    public void ParseListSourcesOutput_ParsesAllSourcesInOrder()
    {
        var sources = FeedResolver.ParseListSourcesOutput(SampleOutput);

        Assert.Equal(3, sources.Count);

        Assert.Equal("nuget.org", sources[0].Name);
        Assert.Equal("https://api.nuget.org/v3/index.json", sources[0].Url);
        Assert.True(sources[0].IsEnabled);

        Assert.Equal("OTP_PreRelease", sources[1].Name);
        Assert.True(sources[1].IsEnabled);

        Assert.Equal("OTP_Release", sources[2].Name);
        Assert.Equal("https://pkgs.dev.azure.com/1touchpoint/_packaging/OTP_Release/nuget/v3/index.json", sources[2].Url);
        Assert.False(sources[2].IsEnabled);
    }

    [Fact]
    public void ParseListSourcesOutput_EmptyInput_ReturnsNoSources()
    {
        var sources = FeedResolver.ParseListSourcesOutput(string.Empty);

        Assert.Empty(sources);
    }
}

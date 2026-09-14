using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class DotnetCliResultTests
{
    [Fact]
    public void GetDiagnosticLines_ExtractsErrorLinesFromStandardOutput()
    {
        // `dotnet add package` writes its real failure text to stdout, with stderr empty.
        const string stdout = """
            info : Restoring packages for /repo/Foo.csproj...
            error: NU1102: Unable to find package Foo.Bar with version (>= 2.0.26254.2-test)
            error:   - Found 3 version(s) in OTP_PreRelease [ Nearest version: 2.0.26126.1-test ]
            """;

        var result = new DotnetCliResult(1, stdout, string.Empty);

        var diagnostics = result.GetDiagnosticLines();

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, line => Assert.Contains("error", line, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetDiagnosticLines_NoErrorLines_FallsBackToFullOutput()
    {
        var result = new DotnetCliResult(1, "something went wrong, no error keyword here", string.Empty);

        var diagnostics = result.GetDiagnosticLines();

        Assert.Single(diagnostics);
        Assert.Equal("something went wrong, no error keyword here", diagnostics[0]);
    }

    [Fact]
    public void GetDiagnosticLines_EmptyOutput_ReturnsEmpty()
    {
        var result = new DotnetCliResult(1, string.Empty, string.Empty);

        Assert.Empty(result.GetDiagnosticLines());
    }
}

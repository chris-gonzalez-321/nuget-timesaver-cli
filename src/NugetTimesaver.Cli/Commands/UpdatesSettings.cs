using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands;

public class UpdatesSettings : CommandSettings
{
    [CommandOption("--feed <FEED>")]
    public string? Feed { get; init; }

    [CommandOption("--fldr <FOLDER>")]
    public string? Folder { get; init; }

    [CommandOption("--pkg-wc <WILDCARD>")]
    public string? PackageWildcard { get; init; }

    [CommandOption("--pre-r")]
    public bool AllowPrerelease { get; init; }

    /// <summary>
    /// For projects whose restore fails (e.g. a version conflict a package update itself would
    /// fix), fall back to reading current versions straight from the .csproj and looking up
    /// latest versions directly against the feed, bypassing restore entirely. Skips the normal
    /// compatibility check `dotnet add package` would otherwise do — run `dotnet restore`
    /// afterward to confirm the result actually resolves.
    /// </summary>
    [CommandOption("--force")]
    public bool Force { get; init; }
}

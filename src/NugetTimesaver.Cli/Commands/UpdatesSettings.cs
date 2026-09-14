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
}

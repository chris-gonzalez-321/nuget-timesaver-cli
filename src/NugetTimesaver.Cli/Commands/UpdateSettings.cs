using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands;

public sealed class UpdateSettings : UpdatesSettings
{
    [CommandOption("--apply")]
    public bool Apply { get; init; }
}

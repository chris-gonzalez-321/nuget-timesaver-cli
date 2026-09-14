using NugetTimesaver.Cli.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands.Feeds;

public sealed class FeedsRemoveSettings : CommandSettings
{
    [CommandArgument(0, "<NAME>")]
    public required string Name { get; init; }
}

public sealed class FeedsRemoveCommand : AsyncCommand<FeedsRemoveSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, FeedsRemoveSettings settings, CancellationToken cancellationToken)
    {
        var result = await DotnetCli.RunAsync("nuget", "remove", "source", settings.Name);

        if (!result.Succeeded)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Failed to remove feed:[/] {result.StandardError.Trim()}");
            return 1;
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Removed feed[/] '{settings.Name}'");
        return 0;
    }
}

using NugetTimesaver.Cli.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands.Feeds;

public sealed class FeedsViewCommand : AsyncCommand
{
    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var sources = await FeedResolver.ListSourcesAsync();

        var table = new Table()
            .AddColumn("Name")
            .AddColumn("URL")
            .AddColumn("Status");

        foreach (var source in sources)
        {
            table.AddRow(
                source.Name,
                source.Url,
                source.IsEnabled ? "[green]Enabled[/]" : "[grey]Disabled[/]");
        }

        AnsiConsole.Write(table);
        return 0;
    }
}

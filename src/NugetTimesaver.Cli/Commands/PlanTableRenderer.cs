using NugetTimesaver.Cli.Core.Models;
using Spectre.Console;

namespace NugetTimesaver.Cli.Commands;

internal static class PlanTableRenderer
{
    public static void Render(IReadOnlyList<PackageUpdate> plan)
    {
        if (plan.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Everything is up to date.[/]");
            return;
        }

        var table = new Table()
            .AddColumn("Project")
            .AddColumn("Package")
            .AddColumn("Current")
            .AddColumn("Latest");

        foreach (var update in plan)
        {
            table.AddRow(
                Path.GetFileNameWithoutExtension(update.ProjectPath),
                update.PackageId,
                update.CurrentVersion,
                update.LatestVersion);
        }

        AnsiConsole.Write(table);
    }
}

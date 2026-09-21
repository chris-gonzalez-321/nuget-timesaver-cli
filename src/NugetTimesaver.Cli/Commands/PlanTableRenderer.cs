using NugetTimesaver.Cli.Core.Models;
using Spectre.Console;

namespace NugetTimesaver.Cli.Commands;

internal static class PlanTableRenderer
{
    public static void Render(UpdatePlanResult plan)
    {
        if (plan.Updates.Count == 0 && plan.Failures.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Everything is up to date.[/]");
            return;
        }

        if (plan.Updates.Count > 0)
        {
            var table = new Table()
                .AddColumn("Project")
                .AddColumn("Package")
                .AddColumn("Current")
                .AddColumn("Latest")
                .AddColumn("");

            foreach (var update in plan.Updates)
            {
                table.AddRow(
                    Path.GetFileNameWithoutExtension(update.ProjectPath),
                    update.PackageId,
                    update.CurrentVersion,
                    update.LatestVersion,
                    update.IsForced ? "[yellow]forced[/]" : "");
            }

            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[green]No updates found for the projects that could be checked.[/]");
        }

        RenderFailures(plan.Failures);
    }

    private static void RenderFailures(IReadOnlyList<ProjectCheckFailure> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"\n[yellow]{failures.Count} project(s) could not be checked:[/]");

        foreach (var failure in failures)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{Path.GetFileNameWithoutExtension(failure.ProjectPath)}[/]:");

            foreach (var line in failure.DiagnosticLines)
            {
                AnsiConsole.MarkupLineInterpolated($"    {line}");
            }
        }
    }
}

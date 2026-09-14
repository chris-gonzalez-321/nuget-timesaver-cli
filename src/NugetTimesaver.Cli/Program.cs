using NugetTimesaver.Cli.Commands;
using NugetTimesaver.Cli.Commands.Feeds;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("nuget-t");

    config.AddBranch("feeds", feeds =>
    {
        feeds.SetDescription("View, add, or remove configured NuGet feeds.");
        feeds.AddCommand<FeedsViewCommand>("view").WithDescription("List configured NuGet feeds.");
        feeds.AddCommand<FeedsAddCommand>("add").WithDescription("Add a NuGet feed.");
        feeds.AddCommand<FeedsRemoveCommand>("remove").WithDescription("Remove a NuGet feed.");
    });

    config.AddCommand<ViewUpdatesCommand>("view-updates")
        .WithDescription("Show available package updates across projects.");

    config.AddCommand<UpdateCommand>("update")
        .WithDescription("Update packages across projects (dry-run unless --apply is passed).");
});

return await app.RunAsync(args);

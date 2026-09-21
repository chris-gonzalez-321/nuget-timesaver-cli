namespace NugetTimesaver.Cli.Core.Models;

/// <summary>A &lt;PackageReference&gt; read straight from a .csproj file, bypassing restore.</summary>
public sealed record DirectPackageReference(string ProjectPath, string PackageId, string Version);

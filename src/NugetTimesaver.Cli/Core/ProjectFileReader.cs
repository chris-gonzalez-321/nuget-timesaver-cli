using System.Xml.Linq;
using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>
/// Reads &lt;PackageReference&gt; entries straight out of a .csproj file's XML. Unlike
/// `dotnet list package`, this needs no restore to succeed — it's the fallback used for
/// projects whose restore is currently broken (e.g. a version conflict that needs fixing
/// via a package update in the first place).
/// </summary>
public static class ProjectFileReader
{
    public static IReadOnlyList<DirectPackageReference> ReadPackageReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);

        return document.Descendants("PackageReference")
            .Select(element => new
            {
                Id = (string?)element.Attribute("Include"),
                Version = (string?)element.Attribute("Version"),
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Id) && !string.IsNullOrWhiteSpace(r.Version))
            .Select(r => new DirectPackageReference(projectPath, r.Id!, r.Version!))
            .ToList();
    }
}

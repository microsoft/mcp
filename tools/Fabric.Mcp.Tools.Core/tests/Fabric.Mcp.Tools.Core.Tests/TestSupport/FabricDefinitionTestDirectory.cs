// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class FabricDefinitionTestDirectory() : IDisposable
{
    internal string Path { get; } = Create();

    public void Dispose() => Directory.Delete(Path, recursive: true);

    internal static string Create()
    {
        // macOS commonly exposes its temporary directory through /var -> /private/var.
        // Resolve the test host's aliases without relaxing the product's link rejection.
        var temporary = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath());
        var current = System.IO.Path.GetPathRoot(temporary)!;
        foreach (var segment in temporary[current.Length..].Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            var directory = new DirectoryInfo(System.IO.Path.Combine(current, segment));
            current = directory.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? directory.FullName;
        }
        return Directory.CreateDirectory(System.IO.Path.Combine(current, $"fabric-definition-tests-{Guid.NewGuid():N}")).FullName;
    }
}

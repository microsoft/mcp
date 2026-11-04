// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Tests.Helpers;

public sealed class TestEnvironmentScope(IReadOnlyDictionary<string, string?> values) : IDisposable
{
    private readonly Dictionary<string, string?> _originalValues = Apply(values);
    private bool _disposed;

    public void SetVariables(IReadOnlyDictionary<string, string?> values)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var (name, value) in values)
        {
            _originalValues.TryAdd(name, Environment.GetEnvironmentVariable(name));
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    private static Dictionary<string, string?> Apply(IReadOnlyDictionary<string, string?> values)
    {
        Dictionary<string, string?> original = new(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        try
        {
            foreach (var (name, value) in values)
            {
                original.Add(name, Environment.GetEnvironmentVariable(name));
                Environment.SetEnvironmentVariable(name, value);
            }
            return original;
        }
        catch
        {
            Restore(original);
            throw;
        }
    }

    private static void Restore(Dictionary<string, string?> values)
    {
        foreach (var (name, value) in values)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Restore(_originalValues);
            _disposed = true;
        }
    }
}

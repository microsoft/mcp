// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests;

public class TestEnvVar : DisposableConfig
{
    private static SemaphoreSlim _lock = new(1, 1);
    public TestEnvVar(string name, string value) : base(name, value, _lock) { }
    public TestEnvVar(Dictionary<string, string> values) : base(values, _lock) { }

    internal override void SetValue(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        _originalValues[name] = Environment.GetEnvironmentVariable(name);

        CleanExistingEnvironmentVariables();

        Environment.SetEnvironmentVariable(name, value);
    }

    internal override void SetValues(Dictionary<string, string> values)
    {
        foreach (var kvp in values)
        {
            _originalValues[kvp.Key] = Environment.GetEnvironmentVariable(kvp.Key);
        }

        CleanExistingEnvironmentVariables();

        foreach (var kvp in values)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }

    internal override void InitValues()
    { }

    // clear the existing values so that the test needs only set up the values relevant to it.
    private void CleanExistingEnvironmentVariables()
    {
        foreach (var kvp in _originalValues)
        {
            Environment.SetEnvironmentVariable(kvp.Key, null);
        }
    }

    internal override void Cleanup()
    {
        foreach (var kvp in _originalValues)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }
}

public abstract class DisposableConfig : IDisposable
{
    private readonly SemaphoreSlim _lock;
    // Common environment variables to be saved off for tests. Add more as needed
    protected readonly Dictionary<string, string?> _originalValues = new();

    public DisposableConfig(string name, string value, SemaphoreSlim sem)
    {
        _lock = sem;
        var acquired = _lock.Wait(TimeSpan.Zero);
        if (!acquired)
        {
            throw new Exception($"Concurrent use of {nameof(TestEnvVar)}. Consider marking these tests to not run in parallel.");
        }

        InitValues();
        SetValue(name, value);
    }

    public DisposableConfig(Dictionary<string, string> values, SemaphoreSlim sem)
    {
        _lock = sem;
        var acquired = _lock.Wait(TimeSpan.Zero);
        if (!acquired)
        {
            throw new Exception($"Concurrent use of {nameof(TestEnvVar)}. Consider marking these tests to not run in parallel.");
        }

        InitValues();
        SetValues(values);
    }

    internal abstract void SetValue(string name, string value);
    internal abstract void SetValues(Dictionary<string, string> values);
    internal abstract void InitValues();
    internal abstract void Cleanup();

    public void Dispose()
    {
        Cleanup();
        _lock.Release();
    }
}

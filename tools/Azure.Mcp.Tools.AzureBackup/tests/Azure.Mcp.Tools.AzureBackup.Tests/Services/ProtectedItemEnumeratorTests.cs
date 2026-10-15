// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class ProtectedItemEnumeratorTests
{
    private const string Context = "Listing protected items in vault 'v'";

    [Fact]
    public async Task CollectToleratingItemFailuresAsync_ReturnsAllItems_WhenNoFailures()
    {
        var enumerator = new FakeAsyncEnumerator<string>(Value("a"), Value("b"), Value("c"));

        var result = await ProtectedItemEnumerator.CollectToleratingItemFailuresAsync(enumerator, x => x, Context);

        Assert.Equal(new[] { "a", "b", "c" }, result);
        Assert.Equal(1, enumerator.DisposeCount);
    }

    [Fact]
    public async Task CollectToleratingItemFailuresAsync_SkipsIsolatedFailure_AndContinues()
    {
        // A single bad item the enumerator can advance past is skipped, not surfaced, so one
        // unsupported backup instance does not blank the whole list.
        var enumerator = new FakeAsyncEnumerator<string>(
            Value("a"),
            Throw(new FormatException("bad item")),
            Value("b"));

        var result = await ProtectedItemEnumerator.CollectToleratingItemFailuresAsync(enumerator, x => x, Context);

        Assert.Equal(new[] { "a", "b" }, result);
        Assert.Equal(1, enumerator.DisposeCount);
    }

    [Fact]
    public async Task CollectToleratingItemFailuresAsync_Throws_WhenFailuresTruncateListing()
    {
        // A page the enumerator cannot advance past throws on every MoveNextAsync; after the
        // consecutive-failure cap the listing is truncated and must surface as an error rather
        // than returning the one item read so far as a success.
        var inner = new FormatException("empty resourceGroupId");
        var enumerator = new FakeAsyncEnumerator<string>(
            Value("a"),
            Throw(inner),
            Throw(new FormatException()),
            Throw(new FormatException()),
            Value("never-reached"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProtectedItemEnumerator.CollectToleratingItemFailuresAsync(enumerator, x => x, Context));

        Assert.Contains("incomplete", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1 item(s)", ex.Message);
        Assert.Contains("resourceGroupId", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
        Assert.Equal(1, enumerator.DisposeCount);
    }

    [Fact]
    public async Task CollectToleratingItemFailuresAsync_Rethrows_OperationCanceled()
    {
        var enumerator = new FakeAsyncEnumerator<string>(
            Value("a"),
            Throw(new OperationCanceledException()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ProtectedItemEnumerator.CollectToleratingItemFailuresAsync(enumerator, x => x, Context));

        Assert.Equal(1, enumerator.DisposeCount);
    }

    [Fact]
    public async Task CollectToleratingItemFailuresAsync_SurfacesMapFailures_AsTruncation()
    {
        // Failures raised while projecting a fetched element are treated the same as enumeration
        // failures: isolated ones are skipped, repeated ones truncate.
        var enumerator = new FakeAsyncEnumerator<string>(Value("x"), Value("x"), Value("x"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProtectedItemEnumerator.CollectToleratingItemFailuresAsync<string, string>(
                enumerator,
                _ => throw new ArgumentException("bad map"),
                Context));

        Assert.Contains("incomplete", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, enumerator.DisposeCount);
    }

    private static Func<string> Value(string value) => () => value;

    private static Func<string> Throw(Exception exception) => () => throw exception;

    private sealed class FakeAsyncEnumerator<T>(params Func<T>[] steps) : IAsyncEnumerator<T>
    {
        private readonly Func<T>[] _steps = steps;
        private int _index = -1;
        private T _current = default!;

        public int DisposeCount { get; private set; }

        public T Current => _current;

        public ValueTask<bool> MoveNextAsync()
        {
            _index++;
            if (_index >= _steps.Length)
            {
                return ValueTask.FromResult(false);
            }

            _current = _steps[_index]();
            return ValueTask.FromResult(true);
        }

        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }
}

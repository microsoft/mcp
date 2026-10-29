// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Unit tests for the internal <see cref="RsvBackupOperations.EnumerateTolerantAsync{TSource, TResult}"/>
/// helper that backs defensive recovery-point enumeration. The RecoveryServicesBackup SDK can throw
/// <see cref="UriFormatException"/>/<see cref="FormatException"/> while deserializing a single
/// Azure Files (<c>FileShareRecoveryPoint</c>) item; the helper must skip the offending item and
/// return the remaining valid items rather than failing the entire list.
/// </summary>
public class RsvBackupOperationsEnumerateTolerantTests
{
    /// <summary>
    /// An <see cref="IAsyncEnumerable{T}"/> whose per-item factories can throw to simulate the SDK
    /// failing to deserialize an individual element during <see cref="IAsyncEnumerator{T}.MoveNextAsync"/>.
    /// The enumerator advances its index before invoking the factory, so a throwing item still lets
    /// the next <c>MoveNextAsync</c> yield the following element - matching the SDK's item-level
    /// deserialization failure behavior.
    /// </summary>
    private sealed class FaultyAsyncEnumerable(IReadOnlyList<Func<int>> factories) : IAsyncEnumerable<int>
    {
        public bool Disposed { get; private set; }

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, factories);

        private sealed class Enumerator(FaultyAsyncEnumerable owner, IReadOnlyList<Func<int>> factories) : IAsyncEnumerator<int>
        {
            private int _index = -1;

            public int Current { get; private set; }

            public ValueTask<bool> MoveNextAsync()
            {
                _index++;
                if (_index >= factories.Count)
                {
                    return ValueTask.FromResult(false);
                }

                Current = factories[_index]();
                return ValueTask.FromResult(true);
            }

            public ValueTask DisposeAsync()
            {
                owner.Disposed = true;
                return ValueTask.CompletedTask;
            }
        }
    }

    private static Func<int> Good(int value) => () => value;
    private static Func<int> ThrowsFormat() => () => throw new FormatException("bad duration");
    private static Func<int> ThrowsUri() => () => throw new UriFormatException("bad uri");

    [Fact]
    public async Task EnumerateTolerantAsync_AllValid_ReturnsAllItems()
    {
        var source = new FaultyAsyncEnumerable([Good(1), Good(2), Good(3)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(source, static x => x, CancellationToken.None);

        Assert.Equal([1, 2, 3], result);
        Assert.True(source.Disposed);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_SkipsFormatExceptionItem_ReturnsValidItems()
    {
        var source = new FaultyAsyncEnumerable([Good(1), ThrowsFormat(), Good(3)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(source, static x => x, CancellationToken.None);

        Assert.Equal([1, 3], result);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_SkipsUriFormatExceptionItem_ReturnsValidItems()
    {
        // UriFormatException derives from FormatException - the NEW-5 Azure Files case.
        var source = new FaultyAsyncEnumerable([Good(10), ThrowsUri(), Good(20)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(source, static x => x, CancellationToken.None);

        Assert.Equal([10, 20], result);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_SelectorThrowsFormatException_SkipsThatItem()
    {
        var source = new FaultyAsyncEnumerable([Good(1), Good(2), Good(3)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(
            source,
            static x => x == 2 ? throw new FormatException("bad projection") : x,
            CancellationToken.None);

        Assert.Equal([1, 3], result);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_StopsAfterMaxConsecutiveFailures()
    {
        // first valid item, then three consecutive failures hit the cap and enumeration stops
        // before the trailing valid item is ever read.
        var source = new FaultyAsyncEnumerable(
            [Good(1), ThrowsFormat(), ThrowsFormat(), ThrowsFormat(), Good(99)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(
            source, static x => x, CancellationToken.None, maxConsecutiveFailures: 3);

        Assert.Equal([1], result);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_ConsecutiveFailureCountResetsAfterSuccess()
    {
        // failures separated by successes never reach the cap, so every valid item is returned.
        var source = new FaultyAsyncEnumerable(
            [ThrowsFormat(), Good(1), ThrowsFormat(), Good(2), ThrowsFormat(), Good(3)]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(
            source, static x => x, CancellationToken.None, maxConsecutiveFailures: 3);

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_EmptySource_ReturnsEmpty()
    {
        var source = new FaultyAsyncEnumerable([]);

        var result = await RsvBackupOperations.EnumerateTolerantAsync(source, static x => x, CancellationToken.None);

        Assert.Empty(result);
        Assert.True(source.Disposed);
    }

    [Fact]
    public async Task EnumerateTolerantAsync_NullSource_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            RsvBackupOperations.EnumerateTolerantAsync<int, int>(null!, static x => x, CancellationToken.None));
    }

    [Fact]
    public async Task EnumerateTolerantAsync_NullSelector_Throws()
    {
        var source = new FaultyAsyncEnumerable([Good(1)]);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            RsvBackupOperations.EnumerateTolerantAsync<int, int>(source, null!, CancellationToken.None));
    }
}

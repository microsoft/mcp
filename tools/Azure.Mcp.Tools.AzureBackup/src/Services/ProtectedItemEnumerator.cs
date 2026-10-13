// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Helpers for enumerating Azure Backup instance pages that can fail to deserialize.
/// </summary>
internal static class ProtectedItemEnumerator
{
    /// <summary>
    /// Enumerates <paramref name="enumerator"/>, projecting each element with <paramref name="map"/>.
    /// Isolated deserialization failures - a single element the enumerator can advance past - are
    /// skipped so one unsupported backup instance does not blank the whole list. A failure the
    /// enumerator cannot advance past (for example a page whose JSON cannot be deserialized because
    /// a backup instance carries an empty or malformed resourceGroupId) repeats on every
    /// <c>MoveNextAsync</c> call; after <paramref name="maxConsecutiveFailures"/> consecutive
    /// failures the enumeration is treated as truncated and an exception is thrown. This surfaces
    /// the loss to the caller instead of silently returning an incomplete list as a success.
    /// </summary>
    /// <param name="enumerator">The async enumerator to drain. Always disposed before returning.</param>
    /// <param name="map">Projection applied to each successfully fetched element.</param>
    /// <param name="truncationContext">Human-readable prefix describing the listing, used in the truncation message.</param>
    /// <param name="maxConsecutiveFailures">Consecutive deserialization failures tolerated before the listing is considered truncated.</param>
    internal static async Task<List<TResult>> CollectToleratingItemFailuresAsync<TSource, TResult>(
        IAsyncEnumerator<TSource> enumerator,
        Func<TSource, TResult> map,
        string truncationContext,
        int maxConsecutiveFailures = 3)
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        ArgumentNullException.ThrowIfNull(map);

        var results = new List<TResult>();
        var consecutiveFailures = 0;
        try
        {
            while (true)
            {
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        break;
                    }

                    results.Add(map(enumerator.Current));
                    consecutiveFailures = 0;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (
                    ex is FormatException
                    or ArgumentNullException
                    or ArgumentException
                    or InvalidOperationException)
                {
                    if (++consecutiveFailures >= maxConsecutiveFailures)
                    {
                        throw new InvalidOperationException(
                            $"{truncationContext} is incomplete: {results.Count} item(s) were returned before a " +
                            "backup instance repeatedly failed to deserialize (commonly caused by an empty or " +
                            "malformed resourceGroupId on the instance), so the remaining instances could not be " +
                            "enumerated. Retrieve a specific item with 'azurebackup protecteditem get " +
                            "--protected-item <name>', or inspect the vault in the Azure portal.",
                            ex);
                    }
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        return results;
    }
}

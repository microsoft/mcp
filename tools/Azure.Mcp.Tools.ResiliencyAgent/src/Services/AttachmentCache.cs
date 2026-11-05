// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Stores user-approved file snapshots in bounded process memory until the first successful A2A
/// submission, expiration, or process shutdown.
/// </summary>
/// <remarks>
/// Raw bytes are never written to disk. Entries are capped by count and total bytes, expire after
/// the policy lifetime, and are cryptographically zeroed when consumed, expired, or disposed.
/// </remarks>
public sealed class AttachmentCache : IAttachmentCache, IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<string, AgentAttachment> _entries = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;
    private readonly ITimer _cleanupTimer;
    private long _totalBytes;

    public AttachmentCache(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _cleanupTimer = timeProvider.CreateTimer(
            _ =>
            {
                lock (_gate)
                {
                    RemoveExpired();
                }
            },
            state: null,
            dueTime: TimeSpan.FromMinutes(1),
            period: TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Adds one immutable snapshot after path validation and explicit user approval.
    /// </summary>
    public AgentAttachment Add(AgentAttachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        lock (_gate)
        {
            RemoveExpired();
            if (_entries.Count >= AttachmentPolicy.MaxCachedFiles
                || _totalBytes + attachment.Content.LongLength > AttachmentPolicy.MaxCachedRawBytes)
            {
                throw new InvalidOperationException(
                    "The local Azure Resiliency attachment cache is full. Submit the prepared files, " +
                    "or wait for unused attachments to expire, then try again.");
            }

            _entries.Add(attachment.AttachmentId, attachment);
            _totalBytes += attachment.Content.LongLength;
            return attachment;
        }
    }

    /// <summary>
    /// Resolves a complete attachment set atomically, enforcing unique IDs, the ten-file limit, and
    /// the aggregate raw-byte limit. No partial set is returned.
    /// </summary>
    public IReadOnlyList<AgentAttachment> Resolve(IReadOnlyList<string>? attachmentIds)
    {
        if (attachmentIds is null || attachmentIds.Count == 0)
        {
            return [];
        }

        lock (_gate)
        {
            RemoveExpired();

            string[] ids = attachmentIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (ids.Length != attachmentIds.Count)
            {
                throw new ArgumentException(
                    "Attachment IDs must be non-empty and unique.",
                    nameof(attachmentIds));
            }

            if (ids.Length > AttachmentPolicy.MaxFilesPerRequest)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attachmentIds),
                    $"A request can include at most {AttachmentPolicy.MaxFilesPerRequest} files.");
            }

            List<AgentAttachment> resolved = new(ids.Length);
            long totalBytes = 0;
            foreach (string id in ids)
            {
                if (!_entries.TryGetValue(id, out AgentAttachment? attachment))
                {
                    throw new KeyNotFoundException(
                        $"Attachment '{id}' is unknown or expired. Prepare the file again.");
                }

                totalBytes += attachment.Content.LongLength;
                if (totalBytes > AttachmentPolicy.MaxAggregateRawBytes)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(attachmentIds),
                        $"The selected files exceed the aggregate limit of {AttachmentPolicy.MaxAggregateRawBytes} bytes.");
                }

                resolved.Add(attachment);
            }

            return resolved;
        }
    }

    /// <summary>
    /// Consumes attachments after A2A accepts the message and securely clears their byte arrays.
    /// </summary>
    public void Remove(IReadOnlyList<string> attachmentIds)
    {
        lock (_gate)
        {
            foreach (string id in attachmentIds.Distinct(StringComparer.Ordinal))
            {
                RemoveEntry(id);
            }
        }
    }

    private void RemoveExpired()
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        string[] expired = _entries
            .Where(pair => pair.Value.ExpiresAt <= now)
            .Select(pair => pair.Key)
            .ToArray();

        foreach (string id in expired)
        {
            RemoveEntry(id);
        }
    }

    private void RemoveEntry(string id)
    {
        if (!_entries.Remove(id, out AgentAttachment? attachment))
        {
            return;
        }

        _totalBytes -= attachment.Content.LongLength;
        CryptographicOperations.ZeroMemory(attachment.Content);
    }

    /// <summary>
    /// Stops periodic cleanup and securely clears every remaining snapshot.
    /// </summary>
    public void Dispose()
    {
        _cleanupTimer.Dispose();
        lock (_gate)
        {
            foreach (string id in _entries.Keys.ToArray())
            {
                RemoveEntry(id);
            }
        }
    }
}

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

/// <summary>
/// Identifies which server currently owns a session.
/// </summary>
public sealed record SessionOwnerInfo
{
    /// <summary>Unique identifier for the owner (server id, instance id, etc.).</summary>
    public required string OwnerId { get; init; }

    /// <summary>Address (host[:port]) requests should be forwarded to.</summary>
    public required string Address { get; init; }

    /// <summary>Timestamp showing when the owner claimed this session.</summary>
    public DateTimeOffset? ClaimedAt { get; init; }
}

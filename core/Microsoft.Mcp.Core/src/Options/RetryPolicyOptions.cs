// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Core;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Represents retry policy configuration for Azure SDK clients.
/// All value properties are nullable — null means "use SDK default."
/// </summary>
public class RetryPolicyOptions : IComparable<RetryPolicyOptions>, IEquatable<RetryPolicyOptions>
{
    [JsonPropertyName(OptionDefinitions.RetryPolicy.DelayName)]
    [Option(Name = "delay", Description = "Initial delay in seconds between retry attempts. For exponential backoff, this value is used as the base.")]
    public double? DelaySeconds { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.MaxDelayName)]
    [Option(Name = "max-delay", Description = "Maximum delay in seconds between retries, regardless of the retry strategy.")]
    public double? MaxDelaySeconds { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.MaxRetriesName)]
    [Option(Name = "max-retries", Description = "Maximum number of retry attempts for failed operations before giving up.")]
    public int? MaxRetries { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.ModeName)]
    [Option(Name = "mode", Description = "Retry strategy to use. 'fixed' uses consistent delays, 'exponential' increases delay between attempts.")]
    public RetryMode? Mode { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.NetworkTimeoutName)]
    [Option(Name = "network-timeout", Description = "Network operation timeout in seconds. Operations taking longer than this will be cancelled.")]
    public double? NetworkTimeoutSeconds { get; set; }

    // Derived flags indicating which options were explicitly provided by the caller.
    [JsonIgnore]
    public bool HasDelaySeconds => DelaySeconds.HasValue;
    [JsonIgnore]
    public bool HasMaxDelaySeconds => MaxDelaySeconds.HasValue;
    [JsonIgnore]
    public bool HasMaxRetries => MaxRetries.HasValue;
    [JsonIgnore]
    public bool HasMode => Mode.HasValue;
    [JsonIgnore]
    public bool HasNetworkTimeoutSeconds => NetworkTimeoutSeconds.HasValue;

    public static bool AreEqual(RetryPolicyOptions? policy1, RetryPolicyOptions? policy2)
    {
        if (ReferenceEquals(policy1, policy2))
        {
            return true;
        }

        if (policy1 is null || policy2 is null)
        {
            return false;
        }

        return policy1.MaxRetries == policy2.MaxRetries &&
            policy1.Mode == policy2.Mode &&
            policy1.DelaySeconds == policy2.DelaySeconds &&
            policy1.MaxDelaySeconds == policy2.MaxDelaySeconds &&
            policy1.NetworkTimeoutSeconds == policy2.NetworkTimeoutSeconds;
    }

    public int CompareTo(RetryPolicyOptions? other)
    {
        if (other == null)
            return 1;

        var cmp = Nullable.Compare(MaxRetries, other.MaxRetries);
        if (cmp != 0)
            return cmp;

        cmp = Nullable.Compare(Mode, other.Mode);
        if (cmp != 0)
            return cmp;

        cmp = Nullable.Compare(DelaySeconds, other.DelaySeconds);
        if (cmp != 0)
            return cmp;

        cmp = Nullable.Compare(MaxDelaySeconds, other.MaxDelaySeconds);
        if (cmp != 0)
            return cmp;

        return Nullable.Compare(NetworkTimeoutSeconds, other.NetworkTimeoutSeconds);
    }

    public bool Equals(RetryPolicyOptions? other) => AreEqual(this, other);

    public override bool Equals(object? obj) => obj is RetryPolicyOptions other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(MaxRetries, Mode, DelaySeconds, MaxDelaySeconds, NetworkTimeoutSeconds);

    public static bool operator ==(RetryPolicyOptions? left, RetryPolicyOptions? right) => AreEqual(left, right);

    public static bool operator !=(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left == right);

    public static bool operator <(RetryPolicyOptions? left, RetryPolicyOptions? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(RetryPolicyOptions? left, RetryPolicyOptions? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left <= right);

    public static bool operator >=(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left < right);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Core.Options;

/// <summary>
/// Represents retry policy configuration for Azure SDK clients
/// </summary>
public class RetryPolicyOptions : IComparable<RetryPolicyOptions>, IEquatable<RetryPolicyOptions>
{
    [JsonPropertyName(OptionDefinitions.RetryPolicy.DelayName)]
    public double DelaySeconds { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.MaxDelayName)]
    public double MaxDelaySeconds { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.MaxRetriesName)]
    public int MaxRetries { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.ModeName)]
    public RetryMode Mode { get; set; }

    [JsonPropertyName(OptionDefinitions.RetryPolicy.NetworkTimeoutName)]
    public double NetworkTimeoutSeconds { get; set; }

    // Flags indicating which options were explicitly provided by the caller.
    // This allows us to only override the Azure SDK defaults for the specified settings.
    public bool HasDelaySeconds { get; set; }
    public bool HasMaxDelaySeconds { get; set; }
    public bool HasMaxRetries { get; set; }
    public bool HasMode { get; set; }
    public bool HasNetworkTimeoutSeconds { get; set; }

    /// <summary>
    /// Compares this retry policy with another policy to check if all settings match
    /// </summary>
    /// <param name="other">The retry policy to compare with</param>
    /// <returns>True if both policies have identical settings or are both null, false otherwise</returns>
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

        return policy1.HasMaxRetries == policy2.HasMaxRetries && (!policy1.HasMaxRetries || policy1.MaxRetries == policy2.MaxRetries) &&
            policy1.HasMode == policy2.HasMode && (!policy1.HasMode || policy1.Mode == policy2.Mode) &&
            policy1.HasDelaySeconds == policy2.HasDelaySeconds && (!policy1.HasDelaySeconds || policy1.DelaySeconds == policy2.DelaySeconds) &&
            policy1.HasMaxDelaySeconds == policy2.HasMaxDelaySeconds && (!policy1.HasMaxDelaySeconds || policy1.MaxDelaySeconds == policy2.MaxDelaySeconds) &&
            policy1.HasNetworkTimeoutSeconds == policy2.HasNetworkTimeoutSeconds && (!policy1.HasNetworkTimeoutSeconds || policy1.NetworkTimeoutSeconds == policy2.NetworkTimeoutSeconds);
    }

    public int CompareTo(RetryPolicyOptions? other)
    {
        if (other == null)
            return 1;

        // Compare by whether MaxRetries was specified, then value
        var retryComparison = HasMaxRetries.CompareTo(other.HasMaxRetries);
        if (retryComparison != 0)
            return retryComparison;
        retryComparison = MaxRetries.CompareTo(other.MaxRetries);
        if (retryComparison != 0)
            return retryComparison;

        // Then by Mode specification
        var modeComparison = HasMode.CompareTo(other.HasMode);
        if (modeComparison != 0)
            return modeComparison;
        modeComparison = Mode.CompareTo(other.Mode);
        if (modeComparison != 0)
            return modeComparison;

        // Then by delay settings (specified flag then value)
        var delayFlagComparison = HasDelaySeconds.CompareTo(other.HasDelaySeconds);
        if (delayFlagComparison != 0)
            return delayFlagComparison;
        var delayComparison = DelaySeconds.CompareTo(other.DelaySeconds);
        if (delayComparison != 0)
            return delayComparison;

        var maxDelayFlagComparison = HasMaxDelaySeconds.CompareTo(other.HasMaxDelaySeconds);
        if (maxDelayFlagComparison != 0)
            return maxDelayFlagComparison;
        var maxDelayComparison = MaxDelaySeconds.CompareTo(other.MaxDelaySeconds);
        if (maxDelayComparison != 0)
            return maxDelayComparison;

        // Finally by network timeout flag then value
        var networkTimeoutFlagComparison = HasNetworkTimeoutSeconds.CompareTo(other.HasNetworkTimeoutSeconds);
        if (networkTimeoutFlagComparison != 0)
            return networkTimeoutFlagComparison;
        return NetworkTimeoutSeconds.CompareTo(other.NetworkTimeoutSeconds);
    }

    public bool Equals(RetryPolicyOptions? other) => AreEqual(this, other);

    public override bool Equals(object? obj) => obj is RetryPolicyOptions other && Equals(other);

    public override int GetHashCode()
    {
        var h1 = HashCode.Combine(HasMaxRetries, MaxRetries, HasMode, Mode, HasDelaySeconds);
        var h2 = HashCode.Combine(DelaySeconds, HasMaxDelaySeconds, MaxDelaySeconds, HasNetworkTimeoutSeconds, NetworkTimeoutSeconds);
        return HashCode.Combine(h1, h2);
    }

    public static bool operator ==(RetryPolicyOptions? left, RetryPolicyOptions? right) => AreEqual(left, right);

    public static bool operator !=(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left == right);

    public static bool operator <(RetryPolicyOptions? left, RetryPolicyOptions? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(RetryPolicyOptions? left, RetryPolicyOptions? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left <= right);

    public static bool operator >=(RetryPolicyOptions? left, RetryPolicyOptions? right) => !(left < right);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.CacheForRedis;

public class AccessPolicyAssignment
{
    /// <summary> Name of the access policy. </summary>
    public string? AccessPolicyName { get; set; }

    /// <summary> Name of the identity assigned to an access policy. </summary>
    public string? IdentityName { get; set; }

    /// <summary> Provisioning status of the access policy assignment. </summary>
    public string? ProvisioningState { get; set; }
}

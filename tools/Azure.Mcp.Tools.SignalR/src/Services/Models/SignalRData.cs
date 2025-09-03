// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.SignalR.Services.Models;

/// <summary>
/// Represents SignalR service data from Resource Graph API.
/// </summary>
public class SignalRData
{
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public string? ResourceType { get; set; }
    public string? Location { get; set; }
    public string? ResourceGroup { get; set; }
    public SignalRProperties? Properties { get; set; }
    public SignalRSku? Sku { get; set; }
    public SignalRIdentity? Identity { get; set; }

    public static SignalRData? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var data = new SignalRData();

        if (element.TryGetProperty("id", out var idElement))
            data.ResourceId = idElement.GetString();

        if (element.TryGetProperty("name", out var nameElement))
            data.ResourceName = nameElement.GetString();

        if (element.TryGetProperty("type", out var typeElement))
            data.ResourceType = typeElement.GetString();

        if (element.TryGetProperty("location", out var locationElement))
            data.Location = locationElement.GetString();

        if (element.TryGetProperty("resourceGroup", out var rgElement))
            data.ResourceGroup = rgElement.GetString();

        if (element.TryGetProperty("properties", out var propertiesElement))
            data.Properties = SignalRProperties.FromJson(propertiesElement);

        if (element.TryGetProperty("sku", out var skuElement))
            data.Sku = SignalRSku.FromJson(skuElement);

        if (element.TryGetProperty("identity", out var identityElement))
            data.Identity = SignalRIdentity.FromJson(identityElement);

        return data;
    }
}

public class SignalRProperties
{
    public string? ProvisioningState { get; set; }
    public string? HostName { get; set; }
    public int? PublicPort { get; set; }
    public int? ServerPort { get; set; }
    public SignalRNetworkAcls? NetworkACLs { get; set; }

    public static SignalRProperties? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var properties = new SignalRProperties();

        if (element.TryGetProperty("provisioningState", out var provisioningStateElement))
            properties.ProvisioningState = provisioningStateElement.GetString();

        if (element.TryGetProperty("hostName", out var hostNameElement))
            properties.HostName = hostNameElement.GetString();

        if (element.TryGetProperty("publicPort", out var publicPortElement) && publicPortElement.TryGetInt32(out var publicPort))
            properties.PublicPort = publicPort;

        if (element.TryGetProperty("serverPort", out var serverPortElement) && serverPortElement.TryGetInt32(out var serverPort))
            properties.ServerPort = serverPort;

        if (element.TryGetProperty("networkACLs", out var networkAclsElement))
            properties.NetworkACLs = SignalRNetworkAcls.FromJson(networkAclsElement);

        return properties;
    }
}

public class SignalRSku
{
    public string? Name { get; set; }
    public string? Tier { get; set; }
    public int? Capacity { get; set; }

    public static SignalRSku? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var sku = new SignalRSku();

        if (element.TryGetProperty("name", out var nameElement))
            sku.Name = nameElement.GetString();

        if (element.TryGetProperty("tier", out var tierElement))
            sku.Tier = tierElement.GetString();

        if (element.TryGetProperty("capacity", out var capacityElement) && capacityElement.TryGetInt32(out var capacity))
            sku.Capacity = capacity;

        return sku;
    }
}

public class SignalRIdentity
{
    public string? Type { get; set; }
    public string? PrincipalId { get; set; }
    public string? TenantId { get; set; }
    public Dictionary<string, SignalRUserAssignedIdentity>? UserAssignedIdentities { get; set; }

    public static SignalRIdentity? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var identity = new SignalRIdentity();

        if (element.TryGetProperty("type", out var typeElement))
            identity.Type = typeElement.GetString();

        if (element.TryGetProperty("principalId", out var principalIdElement))
            identity.PrincipalId = principalIdElement.GetString();

        if (element.TryGetProperty("tenantId", out var tenantIdElement))
            identity.TenantId = tenantIdElement.GetString();

        if (element.TryGetProperty("userAssignedIdentities", out var uaiElement) && uaiElement.ValueKind == JsonValueKind.Object)
        {
            identity.UserAssignedIdentities = new Dictionary<string, SignalRUserAssignedIdentity>();
            foreach (var property in uaiElement.EnumerateObject())
            {
                var uai = SignalRUserAssignedIdentity.FromJson(property.Value);
                if (uai != null)
                    identity.UserAssignedIdentities[property.Name] = uai;
            }
        }

        return identity;
    }
}

public class SignalRUserAssignedIdentity
{
    public string? PrincipalId { get; set; }
    public string? ClientId { get; set; }

    public static SignalRUserAssignedIdentity? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var uai = new SignalRUserAssignedIdentity();

        if (element.TryGetProperty("principalId", out var principalIdElement))
            uai.PrincipalId = principalIdElement.GetString();

        if (element.TryGetProperty("clientId", out var clientIdElement))
            uai.ClientId = clientIdElement.GetString();

        return uai;
    }
}

public class SignalRNetworkAcls
{
    public string? DefaultAction { get; set; }
    public SignalRNetworkAcl? PublicNetwork { get; set; }
    public List<SignalRPrivateEndpointNetworkAcl>? PrivateEndpoints { get; set; }

    public static SignalRNetworkAcls? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var networkAcls = new SignalRNetworkAcls();

        if (element.TryGetProperty("defaultAction", out var defaultActionElement))
            networkAcls.DefaultAction = defaultActionElement.GetString();

        if (element.TryGetProperty("publicNetwork", out var publicNetworkElement))
            networkAcls.PublicNetwork = SignalRNetworkAcl.FromJson(publicNetworkElement);

        if (element.TryGetProperty("privateEndpoints", out var privateEndpointsElement) && privateEndpointsElement.ValueKind == JsonValueKind.Array)
        {
            networkAcls.PrivateEndpoints = new List<SignalRPrivateEndpointNetworkAcl>();
            foreach (var pe in privateEndpointsElement.EnumerateArray())
            {
                var privateEndpoint = SignalRPrivateEndpointNetworkAcl.FromJson(pe);
                if (privateEndpoint != null)
                    networkAcls.PrivateEndpoints.Add(privateEndpoint);
            }
        }

        return networkAcls;
    }
}

public class SignalRNetworkAcl
{
    public List<string>? Allow { get; set; }
    public List<string>? Deny { get; set; }

    public static SignalRNetworkAcl? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var networkAcl = new SignalRNetworkAcl();

        if (element.TryGetProperty("allow", out var allowElement) && allowElement.ValueKind == JsonValueKind.Array)
        {
            networkAcl.Allow = new List<string>();
            foreach (var item in allowElement.EnumerateArray())
            {
                var value = item.GetString();
                if (value != null)
                    networkAcl.Allow.Add(value);
            }
        }

        if (element.TryGetProperty("deny", out var denyElement) && denyElement.ValueKind == JsonValueKind.Array)
        {
            networkAcl.Deny = new List<string>();
            foreach (var item in denyElement.EnumerateArray())
            {
                var value = item.GetString();
                if (value != null)
                    networkAcl.Deny.Add(value);
            }
        }

        return networkAcl;
    }
}

public class SignalRPrivateEndpointNetworkAcl : SignalRNetworkAcl
{
    public string? Name { get; set; }

    public new static SignalRPrivateEndpointNetworkAcl? FromJson(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var acl = new SignalRPrivateEndpointNetworkAcl();

        if (element.TryGetProperty("name", out var nameElement))
            acl.Name = nameElement.GetString();

        if (element.TryGetProperty("allow", out var allowElement) && allowElement.ValueKind == JsonValueKind.Array)
        {
            acl.Allow = new List<string>();
            foreach (var item in allowElement.EnumerateArray())
            {
                var value = item.GetString();
                if (value != null)
                    acl.Allow.Add(value);
            }
        }

        if (element.TryGetProperty("deny", out var denyElement) && denyElement.ValueKind == JsonValueKind.Array)
        {
            acl.Deny = new List<string>();
            foreach (var item in denyElement.EnumerateArray())
            {
                var value = item.GetString();
                if (value != null)
                    acl.Deny.Add(value);
            }
        }

        return acl;
    }
}

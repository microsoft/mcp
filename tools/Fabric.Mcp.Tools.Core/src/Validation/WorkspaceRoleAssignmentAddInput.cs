// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceRoleAssignmentAddInput
{
    internal const int MaxRequestBytes = FabricItemJsonInput.MaxObjectBytes;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static (Guid WorkspaceId, Guid PrincipalId, string PrincipalType, string Body) CreateRequest(
        string workspaceId, string principal, string role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new FabricItemInputException("--workspace-id must be a nonempty UUID.");
        }
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new FabricItemInputException("--role is required and must be a nonempty string. No access level is inferred.");
        }
        try
        {
            if (role.Length > MaxRequestBytes || StrictUtf8.GetByteCount(role) > MaxRequestBytes)
            {
                throw new FabricItemInputException("--role exceeds the 1 MiB request budget.");
            }
            var value = FabricItemJsonInput.ParseObject(principal, "--principal")
                ?? throw new FabricItemInputException("--principal is required. Supply the intended principal id and type explicitly.");
            FabricCoreHttpHelpers.ValidateMetadataJson(value);
            ValidatePrincipal(value);
            var body = JsonSerializer.Serialize(new AddWorkspaceRoleAssignmentRequest(value, role),
                CoreJsonContext.Default.AddWorkspaceRoleAssignmentRequest);
            if (Encoding.UTF8.GetByteCount(body) > MaxRequestBytes)
            {
                throw new FabricItemInputException("The serialized role-assignment request must not exceed the 1 MiB (1,048,576 byte) tool budget.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return (workspace, value.GetProperty("id").GetGuid(), value.GetProperty("type").GetString()!, body);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or EncoderFallbackException or
            ArgumentException { InnerException: EncoderFallbackException })
        {
            throw new FabricItemInputException("Invalid principal JSON. Supply a nonempty id UUID and type, with only the documented writable groupDetails or servicePrincipalProfileDetails for that type. Use valid Unicode; read-only, unknown, and duplicate fields are not accepted.");
        }
    }

    private static void ValidatePrincipal(JsonElement value)
    {
        ValidateFields(value, ["id", "type", "groupDetails", "servicePrincipalProfileDetails"]);
        if (!value.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
            !id.TryGetGuid(out var principalId) || principalId == Guid.Empty ||
            !value.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(type.GetString()))
        {
            throw new JsonException();
        }
        if (value.TryGetProperty("groupDetails", out var group) && group.ValueKind != JsonValueKind.Null)
        {
            if (!type.ValueEquals("Group"))
            {
                throw new JsonException();
            }
            ValidateFields(group, ["groupType"]);
            if (group.TryGetProperty("groupType", out var groupType) && groupType.ValueKind != JsonValueKind.Null &&
                (groupType.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(groupType.GetString())))
            {
                throw new JsonException();
            }
        }
        if (value.TryGetProperty("servicePrincipalProfileDetails", out var profile) && profile.ValueKind != JsonValueKind.Null)
        {
            if (!type.ValueEquals("ServicePrincipalProfile"))
            {
                throw new JsonException();
            }
            ValidateFields(profile, ["parentPrincipal"]);
            if (profile.TryGetProperty("parentPrincipal", out var parent) && parent.ValueKind != JsonValueKind.Null)
            {
                ValidatePrincipal(parent);
            }
        }
    }

    private static void ValidateFields(JsonElement value, string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object ||
            value.EnumerateObject().Any(property => !allowed.Contains(property.Name, StringComparer.Ordinal)))
        {
            throw new JsonException();
        }
    }
}

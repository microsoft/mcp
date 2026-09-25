// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class DataAccessRoleCreateOrUpdateOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public required string ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.RoleName)]
    public string? RoleName { get; set; }

    [Option(Description = """
            JSON definition of the data access role. Must include 'name', 'members'
            (with microsoftEntraMembers), and 'decisionRules'.
            members.microsoftEntraMembers[].objectId accepts EITHER an Entra object ID
            (GUID) OR an email address / UPN — non-GUID values are automatically
            resolved to object IDs via Microsoft Graph (tries /users first, then
            /groups by mail, so mail-enabled groups and DLs work too). Do NOT call
            Graph yourself to convert emails to GUIDs first; pass the email or UPN
            directly. tenantId may be omitted — it is filled in during resolution.
            To scope access to a specific folder, include a Path attribute in
            decisionRules. Omitting Path grants access to the entire item.
            Example with emails (preferred when you know the address, not the GUID):
            {"name":"ImagesReadOnly",
             "members":{"microsoftEntraMembers":[
               {"objectId":"alice@contoso.com"},
               {"objectId":"data-readers@contoso.com"}]},
             "decisionRules":[{"effect":"Permit","permission":[
               {"attributeName":"Action","attributeValueIncludedIn":["Read"]},
               {"attributeName":"Path","attributeValueIncludedIn":["Files/images/*"]}]}]}
            Example with GUIDs (use when you already have the object ID):
            {"name":"ImagesReadOnly",
             "members":{"microsoftEntraMembers":[
               {"objectId":"514402e2-4238-4672-b021-ff9000307b66"}]},
             "decisionRules":[{"effect":"Permit","permission":[
               {"attributeName":"Action","attributeValueIncludedIn":["Read"]}]}]}
            """)]
    public string? RoleDefinition { get; set; }

    [Option(Description = "Comma-separated Entra member identifiers (object IDs, emails, or UPNs). Non-GUID values are resolved via Microsoft Graph.")]
    public string? EntraMembers { get; set; }

    [Option(Description = "Comma-separated Fabric item member references in format 'itemId:permission' (e.g. 'dfbe1234-...:Read').")]
    public string? FabricItemMembers { get; set; }

    [Option(Description = "Comma-separated paths to grant access to (e.g. 'Files/images/*,Tables/sales'). Omit to grant access to the entire item.")]
    public string? PermittedPaths { get; set; }

    [Option(Description = "Comma-separated actions to permit. Currently only 'Read' is supported. Defaults to 'Read' if omitted.")]
    public string? PermittedActions { get; set; }
}

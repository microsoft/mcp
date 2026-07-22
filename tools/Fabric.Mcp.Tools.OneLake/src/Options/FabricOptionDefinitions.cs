// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.OneLake.Options;

public static class FabricOptionDefinitions
{

    // Workspace options
    public const string WorkspaceName = "workspace";
    public static readonly Option<string> Workspace = new($"--{WorkspaceName}")
    {
        Description = "The name or ID of the Microsoft Fabric workspace.",
        Required = true
    };

    public const string WorkspaceIdName = "workspace-id";
    public static readonly Option<string> WorkspaceId = new($"--{WorkspaceIdName}")
    {
        Description = "The ID of the Microsoft Fabric workspace.",
        Required = true
    };

    // Item options
    public const string ItemName = "item";
    public static readonly Option<string> Item = new($"--{ItemName}")
    {
        Description = "The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').",
        Required = false
    };

    public const string ItemIdName = "item-id";
    public static readonly Option<string> ItemId = new($"--{ItemIdName}")
    {
        Description = "The ID of the Fabric item.",
        Required = true
    };

    public const string ItemTypeName = "item-type";
    public static readonly Option<string> ItemType = new($"--{ItemTypeName}")
    {
        Description = "The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).",
        Required = false
    };

    // Lakehouse options
    public const string LakehouseIdName = "lakehouse-id";
    public static readonly Option<string> LakehouseId = new($"--{LakehouseIdName}")
    {
        Description = "The ID of the Lakehouse.",
        Required = true
    };

    // File path options
    public const string FilePathName = "file-path";
    public static readonly Option<string> FilePath = new($"--{FilePathName}")
    {
        Description = "The path to the file in OneLake.",
        Required = true
    };

    public const string DirectoryPathName = "directory-path";
    public static readonly Option<string> DirectoryPath = new($"--{DirectoryPathName}")
    {
        Description = "The path to the directory in OneLake.",
        Required = false
    };

    public const string PathName = "path";
    public static readonly Option<string> Path = new($"--{PathName}")
    {
        Description = "The path to list in OneLake storage (optional, defaults to root).",
        Required = false
    };

    // Data operation options
    public const string RecursiveName = "recursive";
    public static readonly Option<bool> Recursive = new($"--{RecursiveName}")
    {
        Description = "Whether to perform the operation recursively.",
        Required = false
    };

    public const string OverwriteName = "overwrite";
    public static readonly Option<bool> Overwrite = new($"--{OverwriteName}")
    {
        Description = "Whether to overwrite existing files.",
        Required = false
    };

    public const string ContentName = "content";
    public static readonly Option<string> Content = new($"--{ContentName}")
    {
        Description = "The content to write to the file.",
        Required = false
    };

    public const string LocalFilePathName = "local-file-path";
    public static readonly Option<string> LocalFilePath = new($"--{LocalFilePathName}", "--local-path")
    {
        Description = "The path to a local file to upload.",
        Required = false
    };

    public const string DownloadFilePathName = "download-file-path";
    public static readonly Option<string> DownloadFilePath = new($"--{DownloadFilePathName}")
    {
        Description = "Local path to save the downloaded content when running locally.",
        Required = false
    };

    public const string ContentTypeName = "content-type";
    public static readonly Option<string> ContentType = new($"--{ContentTypeName}")
    {
        Description = "MIME content type to set on the uploaded file (e.g., 'application/json'). Defaults to 'application/octet-stream'.",
        Required = false
    };

    // Display options
    public const string DisplayNameName = "display-name";
    public static readonly Option<string> DisplayName = new($"--{DisplayNameName}")
    {
        Description = "The display name for the item.",
        Required = true
    };

    public const string DescriptionName = "description";
    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "The description for the item.",
        Required = false
    };

    // Connection options
    public const string ConnectionIdName = "connection-id";
    public static readonly Option<string> ConnectionId = new($"--{ConnectionIdName}")
    {
        Description = "The connection ID for external data sources.",
        Required = false
    };

    // Pagination options
    public const string ContinuationTokenName = "continuation-token";
    public static readonly Option<string> ContinuationToken = new($"--{ContinuationTokenName}")
    {
        Description = "Token for retrieving the next page of results.",
        Required = false
    };

    // Endpoint options
    public const string EndpointTypeName = "endpoint-type";
    public static readonly Option<string> EndpointType = new($"--{EndpointTypeName}")
    {
        Description = "The endpoint type to use for listing items (fabric-api, blob, auto). Default is 'auto' which uses blob endpoint when appropriate.",
        Required = false
    };

    // Table namespace options
    public const string NamespaceName = "namespace";
    public static readonly Option<string> Namespace = new($"--{NamespaceName}")
    {
        Description = "The table namespace (schema) to inspect within the OneLake table API.",
        Required = true
    };

    public const string SchemaName = "schema";
    public static readonly Option<string> Schema = new($"--{SchemaName}")
    {
        Description = "Alias for --namespace when specifying table schemas in the OneLake table API.",
        Required = false
    };

    public const string TableName = "table";
    public static readonly Option<string> Table = new($"--{TableName}")
    {
        Description = "The table name exposed by the OneLake table API.",
        Required = true
    };

    // Data access security options
    public const string RoleNameName = "role-name";
    public static readonly Option<string> RoleName = new($"--{RoleNameName}")
    {
        Description = "The name of the data access role.",
        Required = true
    };

    public const string RoleDefinitionName = "role-definition";
    public static readonly Option<string> RoleDefinition = new($"--{RoleDefinitionName}")
    {
        Description = """
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
            """,
        Required = false
    };

    public const string EntraMembersName = "entra-members";
    public static readonly Option<string> EntraMembers = new($"--{EntraMembersName}")
    {
        Description = "Comma-separated Entra member identifiers (object IDs, emails, or UPNs). Non-GUID values are resolved via Microsoft Graph.",
        Required = false
    };

    public const string FabricItemMembersName = "fabric-item-members";
    public static readonly Option<string> FabricItemMembers = new($"--{FabricItemMembersName}")
    {
        Description = "Comma-separated Fabric item member references in format 'itemId:permission' (e.g. 'dfbe1234-...:Read').",
        Required = false
    };

    public const string PermittedPathsName = "permitted-paths";
    public static readonly Option<string> PermittedPaths = new($"--{PermittedPathsName}")
    {
        Description = "Comma-separated paths to grant access to (e.g. 'Files/images/*,Tables/sales'). Omit to grant access to the entire item.",
        Required = false
    };

    public const string PermittedActionsName = "permitted-actions";
    public static readonly Option<string> PermittedActions = new($"--{PermittedActionsName}")
    {
        Description = "Comma-separated actions to permit. Currently only 'Read' is supported. Defaults to 'Read' if omitted.",
        Required = false
    };

    // Shortcut options
    public const string ShortcutNameName = "shortcut-name";
    public static readonly Option<string> ShortcutName = new($"--{ShortcutNameName}")
    {
        Description = "The name of the shortcut.",
        Required = true
    };

    public const string ShortcutPathName = "shortcut-path";
    public static readonly Option<string> ShortcutPath = new($"--{ShortcutPathName}")
    {
        Description = "The path of the shortcut within the item.",
        Required = true
    };

    public const string ParentPathName = "parent-path";
    public static readonly Option<string> ParentPath = new($"--{ParentPathName}")
    {
        Description = "The parent path under which to list shortcuts.",
        Required = false
    };

    public const string ShortcutConflictPolicyName = "shortcut-conflict-policy";
    public static readonly Option<string> ShortcutConflictPolicy = CreateShortcutConflictPolicyOption();

    private static Option<string> CreateShortcutConflictPolicyOption()
    {
        var option = new Option<string>($"--{ShortcutConflictPolicyName}")
        {
            Description = "Action when a shortcut with the same name and path already exists. Default: Abort.",
            Required = false
        };
        option.AcceptOnlyFromAmong("Abort", "CreateOrOverwrite", "OverwriteOnly", "GenerateUniqueName");
        return option;
    }

    // Settings options
    // Diagnostics flat options
    public const string DiagnosticsStatusName = "status";
    public static readonly Option<string> DiagnosticsStatus = new($"--{DiagnosticsStatusName}")
    {
        Description = "The status of diagnostics: Enabled or Disabled.",
        Required = true
    };

    public const string DestinationLakehouseWorkspaceIdName = "destination-lakehouse-workspace-id";
    public static readonly Option<string> DestinationLakehouseWorkspaceId = new($"--{DestinationLakehouseWorkspaceIdName}")
    {
        Description = "The workspace ID (GUID) of the destination lakehouse for diagnostic logs. Required when --status is Enabled.",
        Required = false
    };

    public const string DestinationLakehouseItemIdName = "destination-lakehouse-item-id";
    public static readonly Option<string> DestinationLakehouseItemId = new($"--{DestinationLakehouseItemIdName}")
    {
        Description = "The item ID (GUID) of the destination lakehouse for diagnostic logs. Required when --status is Enabled.",
        Required = false
    };

    // Immutability policy flat options
    public const string ImmutabilityScopeName = "scope";
    public static readonly Option<string> ImmutabilityScope = new($"--{ImmutabilityScopeName}")
    {
        Description = "The scope of the immutability policy. Currently only 'DiagnosticLogs' is supported.",
        Required = true
    };

    public const string RetentionDaysName = "retention-days";
    public static readonly Option<int> RetentionDays = new($"--{RetentionDaysName}")
    {
        Description = "Number of days to retain diagnostic logs (minimum 1). Cannot be reduced below the current value.",
        Required = true
    };

    // Shortcut target options
    public const string TargetWorkspaceIdName = "target-workspace-id";
    public static readonly Option<string> TargetWorkspaceId = new($"--{TargetWorkspaceIdName}")
    {
        Description = "The workspace ID (GUID) of the target OneLake item.",
        Required = true
    };

    public const string TargetItemIdName = "target-item-id";
    public static readonly Option<string> TargetItemId = new($"--{TargetItemIdName}")
    {
        Description = "The item ID (GUID) of the target OneLake item.",
        Required = true
    };

    public const string TargetPathName = "target-path";
    public static readonly Option<string> TargetPath = new($"--{TargetPathName}")
    {
        Description = "The path within the target item (e.g. 'Files/data').",
        Required = false
    };

    public const string TargetLocationName = "target-location";
    public static readonly Option<string> TargetLocation = new($"--{TargetLocationName}")
    {
        Description = "The target storage URL (e.g. 'https://myaccount.dfs.core.windows.net/container').",
        Required = true
    };

    public const string TargetSubpathName = "target-subpath";
    public static readonly Option<string> TargetSubpath = new($"--{TargetSubpathName}")
    {
        Description = "The subpath within the target storage location.",
        Required = false
    };

    public const string TargetConnectionIdName = "target-connection-id";
    public static readonly Option<string> TargetConnectionId = new($"--{TargetConnectionIdName}")
    {
        Description = "The connection ID (GUID) for authenticating to the target.",
        Required = true
    };

    public const string TargetBucketName = "target-bucket";
    public static readonly Option<string> TargetBucket = new($"--{TargetBucketName}")
    {
        Description = "The bucket name for S3-compatible targets.",
        Required = true
    };

    public const string TargetEnvironmentDomainName = "target-environment-domain";
    public static readonly Option<string> TargetEnvironmentDomain = new($"--{TargetEnvironmentDomainName}")
    {
        Description = "The Dataverse environment domain URI (e.g. 'https://orgname.crm.dynamics.com').",
        Required = true
    };

    public const string TargetDeltaLakeFolderName = "target-deltalake-folder";
    public static readonly Option<string> TargetDeltaLakeFolder = new($"--{TargetDeltaLakeFolderName}")
    {
        Description = "The Delta Lake folder path in Dataverse.",
        Required = true
    };

    public const string TargetTableNameOptionName = "target-table-name";
    public static readonly Option<string> TargetTableName = new($"--{TargetTableNameOptionName}")
    {
        Description = "The Dataverse table name.",
        Required = false
    };

    public const string TargetUpdateFabricItemSensitivityName = "target-update-fabric-item-sensitivity";
    public static readonly Option<bool> TargetUpdateFabricItemSensitivity = new($"--{TargetUpdateFabricItemSensitivityName}")
    {
        Description = "Whether to update Fabric item sensitivity from OneDrive/SharePoint. Default: false.",
        Required = false
    };

    public const string IncludeManagedName = "include-managed";
    public static readonly Option<bool> IncludeManaged = new($"--{IncludeManagedName}")
    {
        Description = "Include DW-managed shortcuts in the results. Default: false (managed shortcuts are hidden to avoid overwhelming output).",
        Required = false
    };
}

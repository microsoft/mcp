// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Options;

/// <summary>
/// Description constants for options shared by the Function App create commands.
/// </summary>
public static class FunctionAppOptionDescriptions
{
    public const string FunctionApp =
        "The globally unique name of the Function App to create. Must be 2-43 characters.";

    public const string Location =
        "The Azure region where the Function App and any auto-provisioned resources are created (e.g., 'eastus', 'westus2').";

    public const string Runtime =
        "The Functions worker runtime (FUNCTIONS_WORKER_RUNTIME). Valid values: dotnet, dotnet-isolated, node, python, java, powershell. Defaults to dotnet.";

    public const string RuntimeVersion =
        "The runtime version for the selected worker (e.g., node: 22, 20; python: 3.12; java: 17). If omitted, a default per runtime is applied.";

    public const string StorageAccount =
        "The name of the Storage account used by the Functions host. It is created in the resource group if it does not exist; if omitted, a name is generated.";

    public const string StorageAuthMode =
        "How the Function App authenticates to its storage account. Valid values: managed-identity (default; enables a system-assigned identity and sets AzureWebJobsStorage__accountName) " +
        "or connection-string (uses an access key). With managed-identity, grant the identity the 'Storage Blob Data Owner' role on the storage account after creation.";
}

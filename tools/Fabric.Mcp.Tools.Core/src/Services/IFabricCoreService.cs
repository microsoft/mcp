// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricCoreService
{
    /// <summary>
    /// Sends one GET to an allowlisted Fabric or Power BI REST path and returns its bounded JSON response,
    /// without retries, redirects, or polling.
    /// </summary>
    /// <param name="api">fabric (default) or powerbi; selects both the host and the token scope.</param>
    /// <param name="path">A relative path starting with /v1/ (Fabric) or /v1.0/myorg/ (Power BI), with an optional query.</param>
    /// <param name="maxResponseBytes">The response size limit; defaults to 1 MiB, maximum 16 MiB.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    Task<ApiResourceGetCommandResult> GetApiResourceAsync(
        string? api,
        string path,
        int? maxResponseBytes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one POST, PUT, PATCH, or DELETE to an allowlisted Fabric or Power BI REST path, without retries or redirects.
    /// Fabric 202 responses return an operation receipt and can be waited on; Power BI 202 responses are never polled.
    /// </summary>
    /// <param name="api">fabric (default) or powerbi; selects both the host and the token scope.</param>
    /// <param name="path">A relative path starting with /v1/ (Fabric) or /v1.0/myorg/ (Power BI), with an optional query.</param>
    /// <param name="method">POST, PUT, PATCH, or DELETE.</param>
    /// <param name="body">An optional JSON object or array, sent unchanged as application/json.</param>
    /// <param name="maxResponseBytes">The response size limit; defaults to 1 MiB, maximum 16 MiB.</param>
    /// <param name="waitOptions">Fabric operation wait options; sync is rejected for Power BI.</param>
    /// <param name="operationResult">Whether to return a waited Fabric operation's JSON result; requires sync.</param>
    /// <param name="cancellationToken">Cancels authentication, the request, or the local wait, not an accepted operation.</param>
    Task<ApiRequestInvokeCommandResult> InvokeApiRequestAsync(
        string? api,
        string path,
        string method,
        string? body = null,
        int? maxResponseBytes = null,
        FabricOperationWaitOptions? waitOptions = null,
        bool operationResult = false,
        CancellationToken cancellationToken = default);

    Task AssignWorkspaceToCapacityAsync(Guid workspaceId, Guid capacityId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves one page of capacities where the principal is an administrator or contributor.</summary>
    /// <param name="continuationToken">An unchanged continuation token, or null for the first page.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    Task<CapacityListResponse> ListCapacitiesAsync(
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Adds one workspace role assignment with one POST, without lookups, retries, conflict updates, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="principal">Explicit principal JSON with id, type, and optional documented writable details.</param>
    /// <param name="role">The caller's unchanged, explicit workspace role intent.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a grant that may already have occurred.</param>
    Task<WorkspaceRoleAssignmentAddCommandResult> AddWorkspaceRoleAssignmentAsync(
        string workspaceId,
        string principal,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one existing workspace role assignment with one DELETE, without lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="workspaceRoleAssignmentId">The nonempty assignment UUID, independent of its principal ID.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a deletion that may already have occurred.</param>
    /// <returns>A receipt only when HTTP 200 confirms deletion, not an assignment or effective-access snapshot.</returns>
    Task<WorkspaceRoleAssignmentDeleteCommandResult> DeleteWorkspaceRoleAssignmentAsync(
        string workspaceId,
        string workspaceRoleAssignmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one existing workspace role assignment with one PATCH, without lookups, creation, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="workspaceRoleAssignmentId">The nonempty assignment UUID, independent of its principal ID.</param>
    /// <param name="role">The caller's unchanged, explicit replacement workspace role.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not an update that may already have occurred.</param>
    Task<WorkspaceRoleAssignmentUpdateCommandResult> UpdateWorkspaceRoleAssignmentAsync(
        string workspaceId,
        string workspaceRoleAssignmentId,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one workspace role assignment with one GET, without listing assignments, identity lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="workspaceRoleAssignmentId">The nonempty assignment UUID, independent of its principal ID.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the read request.</param>
    Task<WorkspaceRoleAssignmentGetCommandResult> GetWorkspaceRoleAssignmentAsync(
        string workspaceId,
        string workspaceRoleAssignmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one complete page of workspace role assignments without directory lookups, group expansion, or implicit paging.</summary>
    /// <param name="workspaceId">The nonempty workspace UUID.</param>
    /// <param name="continuationToken">An unchanged token from a preceding page for the same workspace, or null for the first page.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    Task<WorkspaceRoleAssignmentListCommandResult> ListWorkspaceRoleAssignmentsAsync(
        string workspaceId,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates one logical workspace folder with one POST, without lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty workspace UUID.</param>
    /// <param name="displayName">The unchanged display name, subject to Fabric folder naming rules.</param>
    /// <param name="parentFolderId">An existing nonempty folder UUID; null creates at the workspace root.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a creation that may already have occurred.</param>
    Task<FolderCreateCommandResult> CreateFolderAsync(
        string workspaceId,
        string displayName,
        string? parentFolderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one empty logical workspace folder with one DELETE, without preflight, changing contents, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="folderId">The nonempty UUID of the empty folder to delete.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a deletion that may already have occurred.</param>
    /// <returns>A receipt only when HTTP 200 confirms deletion; no folder snapshot or recovery guarantee.</returns>
    Task<FolderDeleteCommandResult> DeleteFolderAsync(
        string workspaceId,
        string folderId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one logical workspace folder snapshot with one GET, without related lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="folderId">The nonempty UUID of the existing folder.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the read request.</param>
    Task<FolderGetCommandResult> GetFolderAsync(
        string workspaceId,
        string folderId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one complete page of logical workspace folders without walking folders or following continuation URLs.</summary>
    /// <param name="workspaceId">The nonempty workspace UUID.</param>
    /// <param name="rootFolderId">An optional nonempty root folder UUID; null uses the workspace root.</param>
    /// <param name="recursive">Whether to include nested folders; null preserves the API default of true.</param>
    /// <param name="continuationToken">An unchanged token from a previous page with the same workspace and filters.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    Task<FolderListCommandResult> ListFoldersAsync(
        string workspaceId,
        string? rootFolderId = null,
        bool? recursive = null,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Moves one logical folder within its workspace with one POST, without lookups, descendant enumeration, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="folderId">The nonempty UUID of the existing folder.</param>
    /// <param name="targetFolderId">An existing destination folder UUID in the same workspace; null moves to the workspace root.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a move that may already have occurred.</param>
    Task<FolderMoveCommandResult> MoveFolderAsync(
        string workspaceId,
        string folderId,
        string? targetFolderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Moves one item and its child items within its workspace with one POST, without lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the existing item; a child cannot be moved independently.</param>
    /// <param name="targetFolderId">An existing destination folder UUID in the same workspace; null sends an empty object to use the root.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a move that may already have occurred.</param>
    /// <returns>The complete returned collection, including children in service order; unavailable metadata is not inferred.</returns>
    Task<ItemMoveCommandResult> MoveItemAsync(
        string workspaceId,
        string itemId,
        string? targetFolderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Renames one logical workspace folder with one PATCH, without lookups, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="folderId">The nonempty UUID of the existing folder.</param>
    /// <param name="displayName">The unchanged required replacement name, subject to Fabric folder naming rules.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not an update that may already have occurred.</param>
    Task<FolderUpdateCommandResult> UpdateFolderAsync(
        string workspaceId,
        string folderId,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<ItemCreateCommandResult> CreateItemAsync(
        string workspaceId,
        CreateItemRequest request,
        FabricOperationWaitOptions? waitOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a typed item definition, optionally waiting for its operation or reading an earlier completed result.</summary>
    Task<ItemDefinitionGetCommandResult> GetItemDefinitionAsync(
        string? workspaceId = null,
        string? itemId = null,
        string? format = null,
        string? operationId = null,
        FabricOperationWaitOptions? waitOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces a known item's complete definition with one POST and optional no-result LRO state polling.</summary>
    Task<ItemDefinitionUpdateCommandResult> UpdateItemDefinitionAsync(
        string workspaceId,
        string itemId,
        UpdateItemDefinitionRequest request,
        bool? updateMetadata = null,
        FabricOperationWaitOptions? waitOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads operation state once, without polling or interpreting HTTP success as operation success.</summary>
    Task<OperationStateResult> GetOperationStateAsync(string operationId, CancellationToken cancellationToken);

    /// <summary>Reads one bounded JSON or empty result, without polling or following response URLs.</summary>
    Task<OperationResult> GetOperationResultAsync(string operationId, CancellationToken cancellationToken);

    Task DeleteItemAsync(
        string workspaceId,
        string itemId,
        bool? hardDelete = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for one existing Fabric capacity.</summary>
    Task<FabricCapacityMetadata> GetCapacityAsync(string capacityId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves generic metadata for one existing Fabric item without reading its data or definition.
    /// </summary>
    /// <param name="workspaceId">The containing workspace ID as a nonempty UUID.</param>
    /// <param name="itemId">The item ID as a nonempty UUID.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    /// <returns>The validated metadata for the requested item and workspace.</returns>
    /// <exception cref="ArgumentException">Either ID is not a nonempty UUID.</exception>
    /// <exception cref="HttpRequestException">Fabric returns a failure or an unexpected success status.</exception>
    /// <exception cref="System.Text.Json.JsonException">Fabric returns invalid item metadata.</exception>
    Task<FabricItemMetadata> GetItemAsync(
        string workspaceId,
        string itemId,
        CancellationToken cancellationToken = default);

    /// <summary>Requests cancellation once; a 202 receipt confirms acceptance, not the job's final state.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobInstanceId">The existing job-instance UUID, not a Fabric operation ID.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request, not the Fabric job.</param>
    Task<ItemJobInstanceCancelCommandResult> CancelItemJobInstanceAsync(
        string workspaceId,
        string itemId,
        string jobInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one job-instance snapshot and retry guidance without polling or retrieving results.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobInstanceId">The nonempty job-instance UUID, not a Fabric operation ID.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    Task<ItemJobInstanceGetCommandResult> GetItemJobInstanceAsync(
        string workspaceId,
        string itemId,
        string jobInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one complete page of job instances without pagination, per-job reads, or mutations.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="continuationToken">An unchanged token from a preceding page for the same IDs, or null for the first page.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    Task<ItemJobInstanceListCommandResult> ListItemJobInstancesAsync(
        string workspaceId,
        string itemId,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates one schedule with explicit timing and enabled intent, without preflight, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="enabled">Whether to enable the schedule. A past start time may trigger a job immediately.</param>
    /// <param name="configuration">Required schedule JSON, including explicit UTC bounds and localTimeZoneId.</param>
    /// <param name="executionData">Optional opaque, static job-input JSON object; null omits the field.</param>
    /// <param name="cancellationToken">Cancels the request, not a schedule that may already have been created.</param>
    Task<ItemScheduleCreateCommandResult> CreateItemScheduleAsync(
        string workspaceId,
        string itemId,
        string jobType,
        bool enabled,
        string configuration,
        string? executionData = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one explicit schedule without a request body; HTTP 200 confirms deletion, not job cancellation.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="scheduleId">The existing schedule UUID, not a job-instance or operation ID.</param>
    /// <param name="cancellationToken">Cancels authentication and the request, not a deletion that may already have occurred.</param>
    Task<ItemScheduleDeleteCommandResult> DeleteItemScheduleAsync(
        string workspaceId,
        string itemId,
        string jobType,
        string scheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates one schedule with the full required payload; no preflight, merging, retries, or polling.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="scheduleId">The existing schedule UUID, not a job-instance or operation ID.</param>
    /// <param name="enabled">Explicit enabled intent. Disabling does not cancel existing jobs.</param>
    /// <param name="configuration">Required full schedule JSON with explicit UTC bounds and localTimeZoneId.</param>
    /// <param name="executionData">Optional opaque JSON object; null omits the field, without a preservation/clearing guarantee.</param>
    /// <param name="cancellationToken">Cancels the request, not an update or job that may already have occurred.</param>
    Task<ItemScheduleUpdateCommandResult> UpdateItemScheduleAsync(
        string workspaceId,
        string itemId,
        string jobType,
        string scheduleId,
        bool enabled,
        string configuration,
        string? executionData = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one explicitly identified schedule snapshot without discovery, retries, polling, or mutations.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="scheduleId">The existing schedule UUID, not a job-instance or operation ID.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the read request.</param>
    Task<ItemScheduleGetCommandResult> GetItemScheduleAsync(
        string workspaceId,
        string itemId,
        string jobType,
        string scheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one complete schedule page without discovery, per-schedule reads, pagination, or mutations.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="continuationToken">An unchanged token for the same IDs and job type, or null for the first page.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    Task<ItemScheduleListCommandResult> ListItemSchedulesAsync(
        string workspaceId,
        string itemId,
        string jobType,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Submits one job without retries or polling; a 202 receipt confirms acceptance, not completion.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item to execute.</param>
    /// <param name="jobType">The explicit workload job type, encoded as one path component.</param>
    /// <param name="executionData">Optional opaque JSON object; null omits the field.</param>
    /// <param name="jobParameters">Optional JSON parameter array; null omits the field.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request, not the submitted job.</param>
    Task<ItemJobRunCommandResult> RunOnDemandItemJobAsync(
        string workspaceId,
        string itemId,
        string jobType,
        string? executionData = null,
        string? jobParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves metadata for one existing workspace without reading its items or data.</summary>
    /// <param name="workspaceId">The workspace ID as a nonempty UUID.</param>
    /// <param name="preferWorkspaceSpecificEndpoints">The endpoint preference, or null to preserve the service default.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    /// <returns>The validated workspace metadata.</returns>
    /// <exception cref="ArgumentException">The workspace ID is not a nonempty UUID.</exception>
    /// <exception cref="HttpRequestException">Fabric returns a failure or an unexpected success status.</exception>
    /// <exception cref="System.Text.Json.JsonException">Fabric returns invalid workspace metadata.</exception>
    Task<FabricWorkspaceMetadata> GetWorkspaceAsync(
        string workspaceId,
        bool? preferWorkspaceSpecificEndpoints = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists one page of item metadata without following continuation URIs.</summary>
    /// <param name="workspaceId">The nonempty workspace UUID.</param>
    /// <param name="type">The optional item-type filter.</param>
    /// <param name="recursive">Whether to include items in nested folders.</param>
    /// <param name="rootFolderId">The optional nonempty root folder UUID.</param>
    /// <param name="continuationToken">The token returned by a preceding page with the same filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One page, including any continuation information returned by Fabric.</returns>
    Task<ItemListResponse> ListItemsAsync(
        string workspaceId,
        string? type = null,
        bool recursive = true,
        string? rootFolderId = null,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates one workspace, optionally assigning an existing capacity and domain in the same request.</summary>
    /// <param name="request">The workspace creation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The workspace metadata and optional Location header, without polling or retries.</returns>
    Task<WorkspaceCreateResult> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes one workspace and the items under it.</summary>
    /// <param name="workspaceId">The nonempty UUID of the workspace to delete.</param>
    /// <param name="cancellationToken">The token that cancels the request.</param>
    /// <returns>A task that completes only after Fabric returns the documented successful response.</returns>
    Task DeleteWorkspaceAsync(string workspaceId, CancellationToken cancellationToken);

    Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);

    Task<WorkspaceListResponse> ListWorkspacesAsync(
        string? roles = null,
        string? continuationToken = null,
        bool? preferWorkspaceSpecificEndpoints = null,
        CancellationToken cancellationToken = default);

    /// <summary>Updates only the supplied display name and description of a Fabric workspace.</summary>
    /// <param name="workspaceId">The nonempty workspace UUID.</param>
    /// <param name="request">At least one property to update; null properties are omitted.</param>
    /// <param name="cancellationToken">The token for canceling the operation.</param>
    /// <returns>The workspace metadata from the synchronous PATCH response.</returns>
    Task<WorkspaceUpdateResponse> UpdateWorkspaceAsync(
        string workspaceId,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates only the supplied display name and description of an existing Fabric item.</summary>
    /// <param name="workspaceId">The nonempty UUID of the containing workspace.</param>
    /// <param name="itemId">The nonempty UUID of the item.</param>
    /// <param name="request">At least one property to update; an empty description clears it.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated item's metadata, without definition or workload-specific properties.</returns>
    Task<ItemUpdateMetadata> UpdateItemAsync(
        string workspaceId,
        string itemId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default);
}

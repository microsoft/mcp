// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;

namespace Azure.Mcp.Tools.Workbooks.Commands;

[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(WorkbookInfo))]
[JsonSerializable(typeof(WorkbookError))]
[JsonSerializable(typeof(WorkbookListResult))]
[JsonSerializable(typeof(WorkbookBatchResult))]
[JsonSerializable(typeof(WorkbookDeleteBatchResult))]
[JsonSerializable(typeof(List<WorkbookInfo>))]
[JsonSerializable(typeof(List<WorkbookError>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(ListWorkbooksCommand.ListWorkbooksCommandResult))]
[JsonSerializable(typeof(ShowWorkbooksCommand.ShowWorkbooksCommandResult))]
[JsonSerializable(typeof(UpdateWorkbooksCommand.UpdateWorkbooksCommandResult))]
[JsonSerializable(typeof(CreateWorkbooksCommand.CreateWorkbooksCommandResult))]
[JsonSerializable(typeof(DeleteWorkbooksCommand.DeleteWorkbooksCommandResult))]
internal partial class WorkbooksJsonContext : JsonSerializerContext;

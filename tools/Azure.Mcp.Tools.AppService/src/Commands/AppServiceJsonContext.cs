// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Models;
using Azure.ResourceManager.AppService.Models;

namespace Azure.Mcp.Tools.AppService.Commands;

[JsonSerializable(typeof(AppSettingsGetCommand.AppSettingsGetCommandResult))]
[JsonSerializable(typeof(AppSettingsUpdateCommand.AppSettingsUpdateCommandResult))]
[JsonSerializable(typeof(DatabaseAddCommand.DatabaseAddCommandResult))]
[JsonSerializable(typeof(DatabaseConnectionInfo))]
[JsonSerializable(typeof(DeploymentGetCommand.DeploymentGetCommandResult))]
[JsonSerializable(typeof(DetectorDiagnoseCommand.DetectorDiagnoseCommandResult))]
[JsonSerializable(typeof(DetectorDetails))]
[JsonSerializable(typeof(DetectorInfo))]
[JsonSerializable(typeof(DetectorListCommand.DetectorListCommandResult))]
[JsonSerializable(typeof(DiagnosticDataset))]
[JsonSerializable(typeof(DiagnosisResults))]
[JsonSerializable(typeof(IList<DiagnosticDataset>))]
[JsonSerializable(typeof(WebappDetails))]
[JsonSerializable(typeof(WebappGetCommand.WebappGetCommandResult))]
[JsonSerializable(typeof(WebappChangeStateCommand.WebappChangeStateCommandResult))]
public partial class AppServiceJsonContext : JsonSerializerContext;

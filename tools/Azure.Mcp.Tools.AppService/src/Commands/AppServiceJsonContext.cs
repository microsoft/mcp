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

[JsonSerializable(typeof(AppSettingsGetCommand.AppSettingsGetResult))]
[JsonSerializable(typeof(AppSettingsUpdateCommand.AppSettingsUpdateResult))]
[JsonSerializable(typeof(DatabaseAddCommand.DatabaseAddResult))]
[JsonSerializable(typeof(DatabaseConnectionInfo))]
[JsonSerializable(typeof(DeploymentGetCommand.DeploymentGetResult))]
[JsonSerializable(typeof(DetectorDiagnoseCommand.DetectorDiagnoseResult))]
[JsonSerializable(typeof(DetectorDetails))]
[JsonSerializable(typeof(DetectorInfo))]
[JsonSerializable(typeof(DetectorListCommand.DetectorListResult))]
[JsonSerializable(typeof(DiagnosticDataset))]
[JsonSerializable(typeof(DiagnosisResults))]
[JsonSerializable(typeof(IList<DiagnosticDataset>))]
[JsonSerializable(typeof(WebappDetails))]
[JsonSerializable(typeof(WebappGetCommand.WebappGetResult))]
public partial class AppServiceJsonContext : JsonSerializerContext;

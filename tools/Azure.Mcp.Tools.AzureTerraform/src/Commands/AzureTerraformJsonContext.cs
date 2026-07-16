// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

[JsonSerializable(typeof(AzureRMDocsGetCommandResult))]
[JsonSerializable(typeof(AzApiDocsGetCommandResult))]
[JsonSerializable(typeof(AzApiExample))]
[JsonSerializable(typeof(List<AzApiExample>))]
[JsonSerializable(typeof(List<ArgumentDetail>))]
[JsonSerializable(typeof(List<AttributeDetail>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(AvmModule))]
[JsonSerializable(typeof(AvmVersion))]
[JsonSerializable(typeof(AvmModuleListCommandResult))]
[JsonSerializable(typeof(AvmVersionListCommandResult))]
[JsonSerializable(typeof(AvmDocumentationGetCommandResult))]
[JsonSerializable(typeof(List<AvmModule>))]
[JsonSerializable(typeof(List<AvmVersion>))]
[JsonSerializable(typeof(AztfexportCommandResult))]
[JsonSerializable(typeof(InstallationHelp))]
[JsonSerializable(typeof(InstallationMethod))]
[JsonSerializable(typeof(List<InstallationMethod>))]
[JsonSerializable(typeof(ConftestCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AzureTerraformJsonContext : JsonSerializerContext;

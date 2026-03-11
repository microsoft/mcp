// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Commands;

[JsonSerializable(typeof(LanguageListResult))]
[JsonSerializable(typeof(List<LanguageListResult>))]
[JsonSerializable(typeof(ProjectTemplateResult))]
[JsonSerializable(typeof(List<ProjectTemplateResult>))]
[JsonSerializable(typeof(TemplateManifest))]
[JsonSerializable(typeof(TemplateManifestEntry))]
[JsonSerializable(typeof(TemplateGetCommandResult))]
[JsonSerializable(typeof(TemplateListResult))]
[JsonSerializable(typeof(FunctionTemplateResult))]
[JsonSerializable(typeof(TemplateSummary))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class FunctionsJsonContext : JsonSerializerContext;

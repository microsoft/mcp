// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FunctionsTemplate.Commands.Template;
using Azure.Mcp.Tools.FunctionsTemplate.Models;

namespace Azure.Mcp.Tools.FunctionsTemplate.Commands;

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
internal partial class FunctionTemplatesJsonContext : JsonSerializerContext;

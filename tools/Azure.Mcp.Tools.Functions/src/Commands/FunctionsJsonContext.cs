// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services;

namespace Azure.Mcp.Tools.Functions.Commands;

/// <summary>
/// AOT-safe JSON serialization context for Functions commands, CDN manifest, and GitHub API.
/// </summary>
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
[JsonSerializable(typeof(Dictionary<string, RuntimeVersionInfo>))]
[JsonSerializable(typeof(GitHubTreeResponse))]
[JsonSerializable(typeof(GitHubTreeItem))]
[JsonSerializable(typeof(List<GitHubTreeItem>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class FunctionsJsonContext : JsonSerializerContext;

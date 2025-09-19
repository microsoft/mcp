// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services.Models;

namespace Azure.Mcp.Tools.Foundry.Commands;

[JsonSerializable(typeof(ModelsListCommand.ModelsListCommandResult))]
[JsonSerializable(typeof(DeploymentsListCommand.DeploymentsListCommandResult))]
[JsonSerializable(typeof(ModelDeploymentCommand.ModelDeploymentCommandResult))]
[JsonSerializable(typeof(KnowledgeIndexListCommand.KnowledgeIndexListCommandResult))]
[JsonSerializable(typeof(KnowledgeIndexSchemaCommand.KnowledgeIndexSchemaCommandResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(ModelCatalogFilter))]
[JsonSerializable(typeof(ModelCatalogRequest))]
[JsonSerializable(typeof(ModelCatalogResponse))]
[JsonSerializable(typeof(ModelDeploymentInformation))]
[JsonSerializable(typeof(ModelInformation))]
[JsonSerializable(typeof(ModelDeploymentResult))]
[JsonSerializable(typeof(KnowledgeIndexInformation))]
[JsonSerializable(typeof(KnowledgeIndexSchema))]
[JsonSerializable(typeof(CognitiveServicesAccountDeploymentData))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class FoundryJsonContext : JsonSerializerContext;

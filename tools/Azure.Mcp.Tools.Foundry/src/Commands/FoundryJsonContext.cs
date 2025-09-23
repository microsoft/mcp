// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace Azure.Mcp.Tools.Foundry.Commands;

[JsonSerializable(typeof(ModelsListCommand.ModelsListCommandResult))]
[JsonSerializable(typeof(DeploymentsListCommand.DeploymentsListCommandResult))]
[JsonSerializable(typeof(ModelDeploymentCommand.ModelDeploymentCommandResult))]
[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(AgentsConnectCommand.AgentsConnectCommandResult))]
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
[JsonSerializable(typeof(CognitiveServicesSku))]
[JsonSerializable(typeof(CognitiveServicesAccountDeploymentProperties))]
[JsonSerializable(typeof(AgentsQueryAndEvaluateCommand.AgentsQueryAndEvaluateCommandResult))]
[JsonSerializable(typeof(List<ChatMessage>))]
[JsonSerializable(typeof(EvaluationResult))]
[JsonSerializable(typeof(AgentsEvaluateCommand.AgentsEvaluateCommandResult))]
[JsonSerializable(typeof(AgentsConnectResult))]
[JsonSerializable(typeof(AgentsQueryAndEvaluateResult))]
[JsonSerializable(typeof(AgentsEvaluateResult))]
[JsonSerializable(typeof(CognitiveServicesAccountDeploymentData))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class FoundryJsonContext : JsonSerializerContext;

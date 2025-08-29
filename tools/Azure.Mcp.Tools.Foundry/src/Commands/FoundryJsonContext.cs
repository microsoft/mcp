// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.ResourceManager.CognitiveServices.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace Azure.Mcp.Tools.Foundry.Commands;

[JsonSerializable(typeof(ModelsListCommand.ModelsListCommandResult))]
[JsonSerializable(typeof(DeploymentsListCommand.DeploymentsListCommandResult))]
[JsonSerializable(typeof(ModelDeploymentCommand.ModelDeploymentCommandResult))]
[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(AgentsConnectCommand.AgentsConnectCommandResult))]
[JsonSerializable(typeof(KnowledgeIndexListCommand.KnowledgeIndexListCommandResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(ModelCatalogFilter))]
[JsonSerializable(typeof(ModelCatalogRequest))]
[JsonSerializable(typeof(ModelCatalogResponse))]
[JsonSerializable(typeof(ModelDeploymentInformation))]
[JsonSerializable(typeof(ModelInformation))]
[JsonSerializable(typeof(ModelDeploymentResult))]
[JsonSerializable(typeof(KnowledgeIndexInformation))]
[JsonSerializable(typeof(CognitiveServicesAccountSku))]
[JsonSerializable(typeof(CognitiveServicesAccountDeploymentProperties))]
[JsonSerializable(typeof(AgentsQueryAndEvaluateCommand.AgentsQueryAndEvaluateCommandResult))]
[JsonSerializable(typeof(List<ChatMessage>))]
[JsonSerializable(typeof(EvaluationResult))]
[JsonSerializable(typeof(AgentsEvaluateCommand.AgentsEvaluateCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class FoundryJsonContext : JsonSerializerContext;

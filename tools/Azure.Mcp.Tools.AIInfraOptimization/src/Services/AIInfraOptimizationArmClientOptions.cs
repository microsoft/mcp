// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.AIInfraOptimization.Services;

/// <summary>
/// Client options for the AI Infra Optimization ARM pipeline. Azure Core default policies and retries apply; no retry settings
/// are exposed.
/// </summary>
internal sealed class AIInfraOptimizationArmClientOptions : ClientOptions;

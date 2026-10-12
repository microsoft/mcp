// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AIInfraOptimization.Commands.VmSku;
using Azure.Mcp.Tools.AIInfraOptimization.Configuration;
using Azure.Mcp.Tools.AIInfraOptimization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AIInfraOptimization;

public class AIInfraOptimizationSetup : IAreaSetup
{
    public string Name => "aiinfraoptimization";

    public string Title => "Azure AI Infra Optimization";

    public void ConfigureServices(IServiceCollection services)
    {
        // Manual key mapping keeps options binding AOT-safe. An absent or blank origin selects automatic routing
        // (global ARM with Canary fallback); any other configured value must be approved or startup validation fails.
        services.AddOptions<AIInfraOptimizationOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                options.ArmIngressOrigin = AIInfraOptimizationArmIngress.NormalizeConfiguredOrigin(
                    configuration[AIInfraOptimizationArmIngress.ConfigurationKey]))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AIInfraOptimizationOptions>, AIInfraOptimizationOptionsValidator>();

        services.AddHttpClient(AIInfraOptimizationArmClient.HttpClientName)
            .ConfigureHttpClient(client => client.Timeout = AIInfraOptimizationArmClient.ClientTimeout)
            .ConfigurePrimaryHttpMessageHandler(CreateNoRedirectHandler);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<AIInfraOptimizationIngressFallbackCache>();
        services.AddSingleton<IAIInfraOptimizationArmClient, AIInfraOptimizationArmClient>();
        services.AddSingleton<IAIInfraOptimizationService, AIInfraOptimizationService>();
        services.AddSingleton<VmSkuRecommendCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var aiInfraOptimization = new CommandGroup(Name,
            "Azure AI Infra Optimization operations - Recommend Azure VM SKUs for AI model inference workloads using Azure AI Infra Optimization " +
            "VM SKU recommendation. Use when sizing GPU or accelerator VMs for a model, comparing cost and latency " +
            "rankings, or checking subscription quota and placement for candidate VM sizes. Requires an Azure " +
            "subscription context.",
            Title);

        var vmSku = new CommandGroup("vmsku", "AI Infra Optimization VM SKU operations - Recommend and rank Azure VM SKUs for AI model inference.");
        aiInfraOptimization.AddSubGroup(vmSku);

        vmSku.AddCommand<VmSkuRecommendCommand>(serviceProvider);

        return aiInfraOptimization;
    }

    // Redirects are never followed so a bearer token is not replayed to a response-provided location.
    internal static HttpMessageHandler CreateNoRedirectHandler() => new HttpClientHandler { AllowAutoRedirect = false };
}

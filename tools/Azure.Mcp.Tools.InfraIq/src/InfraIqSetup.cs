// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Commands.VmSku;
using Azure.Mcp.Tools.InfraIq.Configuration;
using Azure.Mcp.Tools.InfraIq.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.InfraIq;

public class InfraIqSetup : IAreaSetup
{
    public string Name => "infraiq";

    public string Title => "Azure InfraIQ";

    public void ConfigureServices(IServiceCollection services)
    {
        // Manual key mapping keeps options binding AOT-safe. An absent or blank origin defaults to the approved
        // development origin; any other configured value must be approved or startup validation fails.
        services.AddOptions<InfraIqOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                options.ArmIngressOrigin = InfraIqArmIngress.ResolveConfiguredOrigin(
                    configuration[InfraIqArmIngress.ConfigurationKey]))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<InfraIqOptions>, InfraIqOptionsValidator>();

        services.AddHttpClient(InfraIqArmClient.HttpClientName)
            .ConfigureHttpClient(client => client.Timeout = InfraIqArmClient.ClientTimeout)
            .ConfigurePrimaryHttpMessageHandler(CreateNoRedirectHandler);

        services.AddSingleton<IInfraIqArmClient, InfraIqArmClient>();
        services.AddSingleton<IInfraIqService, InfraIqService>();
        services.AddSingleton<VmSkuRecommendCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var infraIq = new CommandGroup(Name,
            "Azure InfraIQ operations - Recommend Azure VM SKUs for AI model inference workloads using Azure InfraIQ " +
            "VM SKU recommendation. Use when sizing GPU or accelerator VMs for a model, comparing cost and latency " +
            "rankings, or checking subscription quota and placement for candidate VM sizes. Requires an Azure " +
            "subscription context.",
            Title);

        var vmSku = new CommandGroup("vmsku", "InfraIQ VM SKU operations - Recommend and rank Azure VM SKUs for AI model inference.");
        infraIq.AddSubGroup(vmSku);

        vmSku.AddCommand<VmSkuRecommendCommand>(serviceProvider);

        return infraIq;
    }

    // Redirects are never followed so a bearer token is not replayed to a response-provided location.
    internal static HttpMessageHandler CreateNoRedirectHandler() => new HttpClientHandler { AllowAutoRedirect = false };
}

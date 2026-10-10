// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.InfraIq.Commands;
using Azure.Mcp.Tools.InfraIq.Commands.VmSku;
using Azure.Mcp.Tools.InfraIq.Exceptions;
using Azure.Mcp.Tools.InfraIq.Models.Common;
using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Models.Response;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;
using Azure.Mcp.Tools.InfraIq.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.InfraIq.Tests.VmSku;

public class VmSkuRecommendCommandTests : SubscriptionCommandUnitTestsBase<VmSkuRecommendCommand, IInfraIqService>
{
    private const string BaseArgs = "--subscription sub1 --location eastus2 --hugging-face-model-id meta-llama/Llama-3.1-70B-Instruct";

    private static readonly VmSkuRecommendResult SuccessResult = new(
        new InfraIqRecommendVmSkuResponse
        {
            Options = [],
            Sizing = new()
            {
                Model = new() { ParameterCount = 1, WeightPrecision = "FP16" },
                Workload = new() { MaxConcurrentRequestsPerReplica = 1, ContextLength = 1 },
                Memory = new() { ModelWeightsGiB = 1, KvCacheGiBPerRequest = 1, KvCacheGiBTotal = 1, ModelAndKvCacheGiB = 2 },
                Basis = new() { ParameterCountSource = "Request", ModelWeightsCalculation = "ArtifactSize", ContextLengthSource = "Request" }
            }
        },
        new InfraIqArmResponseContext("req-1", "client-1", null, null));

    private InfraIqRecommendVmSkuRequestBody? _capturedBody;

    private void ConfigureSuccess()
    {
        Service.RecommendVmSkuAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Do<InfraIqRecommendVmSkuRequestBody>(body => _capturedBody = body),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(SuccessResult);
    }

    private void ConfigureThrows(Exception exception) =>
        Service.RecommendVmSkuAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<InfraIqRecommendVmSkuRequestBody>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("recommend", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
        Assert.Contains("--subscription", command.Description);
    }

    [Fact]
    public void PublicText_DoesNotMentionPreviewOrAllowlistWording()
    {
        var setup = new InfraIqSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(Substitute.For<IAzureService>());
        services.AddSingleton(Substitute.For<ISubscriptionResolver>());
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var root = setup.RegisterCommands(provider);

        var texts = new List<string> { setup.Title, root.Description, Command.Description, Command.Title };
        texts.AddRange(root.SubGroup.Select(group => group.Description));
        texts.AddRange(CommandDefinition.Options.Select(option => option.Description ?? string.Empty));
        texts.AddRange(new[] { 400, 401, 403, 404, 408, 409, 413, 415, 422, 429, 500, 502, 503, 504, 418 }.Select(InfraIqErrorMessages.ForStatus));
        texts.Add(InfraIqArmClient.UnsupportedCloudMessage);
        texts.Add(Azure.Mcp.Tools.InfraIq.Configuration.InfraIqArmIngress.MissingOriginMessage);

        foreach (var text in texts)
        {
            Assert.DoesNotContain("preview", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("allowlist", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Private.InfraIQ", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Command_HasApprovedMetadata()
    {
        Assert.Equal("recommend", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.NotNull(Command.ResultTypeInfo);
    }

    [Fact]
    public void Setup_RegistersInfraIqVmSkuRecommendPath()
    {
        var setup = new InfraIqSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(Substitute.For<IAzureService>());
        services.AddSingleton(Substitute.For<ISubscriptionResolver>());
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        var root = setup.RegisterCommands(provider);

        Assert.Equal("infraiq", setup.Name);
        Assert.Equal("infraiq", root.Name);
        var vmSku = Assert.Single(root.SubGroup);
        Assert.Equal("vmsku", vmSku.Name);
        var recommend = Assert.Single(vmSku.Commands);
        Assert.Equal("recommend", recommend.Key);
        Assert.IsType<VmSkuRecommendCommand>(recommend.Value);
    }

    [Fact]
    public void PublicSurface_DoesNotExposeOperatorOnlyOrUnsupportedOptions()
    {
        var optionNames = CommandDefinition.Options.Select(option => option.Name).ToArray();

        foreach (var forbidden in new[] { "origin", "endpoint", "api-version", "retry", "rpaas", "max-context-length", "subscription-id", "next-link" })
        {
            Assert.DoesNotContain(optionNames, name => name.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
        }

        Assert.Contains("--target-vm-size", optionNames);
        Assert.DoesNotContain("--target-vm-sizes", optionNames);
    }

    [Theory]
    [InlineData(BaseArgs, true)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7000000000", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7000000000 --num-layers 32 --num-key-value-heads 8 --head-dim 128", true)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --parameter-count 7000000000", true)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --num-layers 32 --num-key-value-heads 8 --head-dim 128", true)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --parameter-count 7000000000 --num-layers 32 --num-key-value-heads 8 --head-dim 128", true)]
    [InlineData("--location eastus2 --hugging-face-model-id m/x", false)]
    [InlineData("--subscription sub1 --hugging-face-model-id m/x", false)]
    [InlineData("--subscription sub1 --location eastus2", false)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 0", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-layers 32", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-layers 32 --num-key-value-heads 8", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-key-value-heads 8 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location eastus2 --num-layers 32 --num-key-value-heads 8 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 0 --num-layers 32 --num-key-value-heads 8 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-layers 0 --num-key-value-heads 8 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-layers 32 --num-key-value-heads 0 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location eastus2 --parameter-count 7 --num-layers 32 --num-key-value-heads 8 --head-dim 0", false)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --num-layers 32", false)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --num-layers 32 --num-key-value-heads 8", false)]
    [InlineData("--subscription sub1 --location eastus2 --hugging-face-model-id m/x --num-layers 32 --num-key-value-heads 0 --head-dim 128", false)]
    [InlineData("--subscription sub1 --location ../x --hugging-face-model-id m/x", false)]
    [InlineData($"{BaseArgs} --weight-precision int4", true)]
    [InlineData($"{BaseArgs} --weight-precision FP32", true)]
    [InlineData($"{BaseArgs} --weight-precision FP64", false)]
    [InlineData($"{BaseArgs} --procurement-option spot", true)]
    [InlineData($"{BaseArgs} --procurement-option Reserved", false)]
    [InlineData($"{BaseArgs} --ranking-preference latency", true)]
    [InlineData($"{BaseArgs} --ranking-preference Speed", false)]
    [InlineData($"{BaseArgs} --max-concurrent-requests-per-replica 0", false)]
    [InlineData($"{BaseArgs} --replica-count 0", false)]
    [InlineData($"{BaseArgs} --prompt-tokens 100 --max-output-tokens 50", true)]
    [InlineData($"{BaseArgs} --prompt-tokens 100", false)]
    [InlineData($"{BaseArgs} --max-output-tokens 100", false)]
    [InlineData($"{BaseArgs} --prompt-tokens 0 --max-output-tokens 0", false)]
    [InlineData($"{BaseArgs} --prompt-tokens 131072 --max-output-tokens 0", true)]
    [InlineData($"{BaseArgs} --prompt-tokens 131072 --max-output-tokens 1", false)]
    [InlineData($"{BaseArgs} --prompt-tokens=-1 --max-output-tokens=5", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount 12.5 --max-deployment-cost-per-hour-currency-code USD", true)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount 12.5 --max-deployment-cost-per-hour-currency-code usd", true)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount 12.5", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-currency-code USD", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount 12.5 --max-deployment-cost-per-hour-currency-code EUR", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount 0 --max-deployment-cost-per-hour-currency-code USD", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount=-1 --max-deployment-cost-per-hour-currency-code USD", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount NaN --max-deployment-cost-per-hour-currency-code USD", false)]
    [InlineData($"{BaseArgs} --max-deployment-cost-per-hour-amount Infinity --max-deployment-cost-per-hour-currency-code USD", false)]
    [InlineData($"{BaseArgs} --target-vm-size Standard_A", true)]
    [InlineData($"{BaseArgs} --target-vm-size Standard_A --target-vm-size Standard_B", true)]
    [InlineData($"{BaseArgs} --target-vm-size Standard_A --target-vm-size standard_a", false)]
    [InlineData($"{BaseArgs} --include-quota --include-placement --include-pricing", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            await Service.DidNotReceive().RecommendVmSkuAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<InfraIqRecommendVmSkuRequestBody>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ParameterCountWithoutArchitecture_FailsLocallyWithoutCallingService()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync("--subscription sub1 --location eastus2 --parameter-count 7000000000");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--num-layers", response.Message);
        Assert.Contains("--head-dim", response.Message);
        await Service.DidNotReceive().RecommendVmSkuAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<InfraIqRecommendVmSkuRequestBody>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ErrorResultSerializesRetryAfterInCamelCase()
    {
        ConfigureThrows(new InfraIqArmException(429, "Throttled", null, "req-9", "client-9", "120"));

        var response = await ExecuteCommandAsync(BaseArgs);

        var json = JsonSerializer.Serialize(response.Results);
        Assert.Contains("\"retryAfter\":\"120\"", json);
        Assert.DoesNotContain("\"RetryAfter\"", json);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsBlankHuggingFaceModelId()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--location", "eastus2", "--hugging-face-model-id", "  ", "--parameter-count", "7");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsAnyNonblankHuggingFaceModelId_WithoutLocalResolution()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--location", "eastus2", "--hugging-face-model-id", "not/a-real-model-id");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("not/a-real-model-id", _capturedBody!.Model!.HuggingFaceModelId);
    }

    [Theory]
    [InlineData(51, 10, HttpStatusCode.BadRequest)]
    [InlineData(50, 10, HttpStatusCode.OK)]
    [InlineData(1, 129, HttpStatusCode.BadRequest)]
    [InlineData(1, 128, HttpStatusCode.OK)]
    public async Task ExecuteAsync_EnforcesTargetVmSizeCountAndLengthLimits(int count, int length, HttpStatusCode expected)
    {
        ConfigureSuccess();
        var values = string.Join(' ', Enumerable.Range(0, count).Select(i => $"--target-vm-size {i.ToString().PadLeft(length, 'v')}"));

        var response = await ExecuteCommandAsync($"{BaseArgs} {values}");

        Assert.Equal(expected, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsIncludeFlagsToFalseAndOmitsEmptyGroups()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync(BaseArgs);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(_capturedBody, InfraIqJsonContext.Default.InfraIqRecommendVmSkuRequestBody);
        Assert.Equal(
            "{\"model\":{\"huggingFaceModelId\":\"meta-llama/Llama-3.1-70B-Instruct\"},\"subscriptionOptions\":{\"includeQuota\":false,\"includePlacement\":false,\"includePricing\":false}}",
            json);
    }

    [Fact]
    public async Task ExecuteAsync_BuildsCanonicalRequestAndForwardsSubscriptionTenantAndCancellation()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync(
            "--subscription sub1 --tenant tenant1 --location \"East US 2\" --parameter-count 70000000000 --num-layers 80 --num-key-value-heads 8 --head-dim 128 " +
            "--weight-precision fp8 --max-concurrent-requests-per-replica 4 --prompt-tokens 1000 --max-output-tokens 200 --replica-count 3 " +
            "--procurement-option spot --ranking-preference latency --max-deployment-cost-per-hour-amount 99.5 --max-deployment-cost-per-hour-currency-code usd " +
            "--target-vm-size \" Standard_A \" --target-vm-size Standard_B --include-quota --include-pricing");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RecommendVmSkuAsync(
            "sub1",
            "eastus2",
            Arg.Any<InfraIqRecommendVmSkuRequestBody>(),
            "tenant1",
            TestContext.Current.CancellationToken);

        var body = _capturedBody!;
        Assert.Null(body.Model!.HuggingFaceModelId);
        Assert.Equal(70000000000, body.Model.ParameterCount);
        Assert.Equal("FP8", body.Model.WeightPrecision);
        Assert.Equal(80, body.Model.NumLayers);
        Assert.Equal(8, body.Model.NumKeyValueHeads);
        Assert.Equal(128, body.Model.HeadDim);
        Assert.Equal(4, body.Workload!.MaxConcurrentRequestsPerReplica);
        Assert.Equal(1000, body.Workload.TokenBudget!.PromptTokens);
        Assert.Equal(200, body.Workload.TokenBudget.MaxOutputTokens);
        Assert.Equal(3, body.Deployment!.ReplicaCount);
        Assert.Equal("Spot", body.Deployment.ProcurementOption);
        Assert.Equal("Latency", body.Optimization!.RankingPreference);
        Assert.Equal(99.5, body.Optimization.MaxDeploymentCostPerHour!.Amount);
        Assert.Equal("USD", body.Optimization.MaxDeploymentCostPerHour.CurrencyCode);
        Assert.Equal(["Standard_A", "Standard_B"], body.TargetVmSizes);
        Assert.True(body.SubscriptionOptions!.IncludeQuota);
        Assert.False(body.SubscriptionOptions.IncludePlacement);
        Assert.True(body.SubscriptionOptions.IncludePricing);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCompleteStructuredResult()
    {
        ConfigureSuccess();

        var response = await ExecuteCommandAsync(BaseArgs);

        var result = ValidateAndDeserializeResponse(response, InfraIqJsonContext.Default.VmSkuRecommendResult);
        Assert.Empty(result.Response.Options);
        Assert.Equal("req-1", result.Context.RequestId);
    }

    [Fact]
    public async Task ExecuteAsync_InfraIqArmException_ReturnsSafeSourceGeneratedErrorResult()
    {
        ConfigureThrows(new InfraIqArmException(429, "Throttled", "model", "req-9", "client-9", "Wed, 21 Oct 2026 07:28:00 GMT"));

        var response = await ExecuteCommandAsync(BaseArgs);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Equal(InfraIqErrorMessages.ForStatus(429), response.Message);
        var error = DeserializeResponse(response, InfraIqJsonContext.Default.VmSkuRecommendErrorResult);
        Assert.NotNull(error);
        Assert.Equal(429, error.Status);
        Assert.Equal("Throttled", error.Code);
        Assert.Equal("model", error.Target);
        Assert.Equal("req-9", error.RequestId);
        Assert.Equal("client-9", error.ClientRequestId);
        Assert.Equal("Wed, 21 Oct 2026 07:28:00 GMT", error.RetryAfter);
        Assert.Equal(InfraIqErrorMessages.ForStatus(429), error.Message);
        Assert.DoesNotContain("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InfraIqArmException_RedirectStatusMapsToBadGateway()
    {
        ConfigureThrows(new InfraIqArmException(302, null, null, null, null, null));

        var response = await ExecuteCommandAsync(BaseArgs);

        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Equal(302, DeserializeResponse(response, InfraIqJsonContext.Default.VmSkuRecommendErrorResult)!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NonInfraIqException_DelegatesToBaseHandling()
    {
        ConfigureThrows(new InvalidOperationException("origin not configured"));

        var response = await ExecuteCommandAsync(BaseArgs);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("origin not configured", response.Message);
        Assert.Contains("https://aka.ms/azmcp/troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HttpRequestException_DelegatesToBaseHandling()
    {
        ConfigureThrows(new HttpRequestException("network down", null, HttpStatusCode.ServiceUnavailable));

        var response = await ExecuteCommandAsync(BaseArgs);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("https://aka.ms/azmcp/troubleshooting", response.Message);
    }
}

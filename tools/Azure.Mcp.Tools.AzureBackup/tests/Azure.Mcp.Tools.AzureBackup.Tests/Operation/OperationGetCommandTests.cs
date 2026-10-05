// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore fother

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Operation;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Operation;

public class OperationGetCommandTests : SubscriptionCommandUnitTestsBase<OperationGetCommand, IRsvBackupOperationService>
{
    private const string Arguments = "--subscription sub --vault vault --resource-group rg --operation opaque:token+v1=";

    [Fact]
    public void Constructor_InitializesReadOnlyRsvCommand()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.Contains("RSV", CommandDefinition.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.DoesNotContain(CommandDefinition.Options, o => o.Name == "--vault-type");
        foreach (var name in new[] { "--operation", "--vault", "--resource-group", "--subscription" })
        {
            Assert.True(Assert.Single(CommandDefinition.Options, o => o.Name == name).Required);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_BindsScopeTenantAndOpaqueOperation(bool itemScope)
    {
        var expected = new BackupOperationInfo("opaque:token+v1=", "/operation", itemScope ? "protectedItem" : "vault", "rsv",
            "Succeeded", DateTimeOffset.Parse("2026-01-01T00:00:00Z"), null, null, "job-one", ["job-two"], new Dictionary<string, string>());
        Service.GetOperationAsync("opaque:token+v1=", "vault", "rg", "sub",
            itemScope ? "IaasVMContainer;v2;rg;vm" : null, itemScope ? "VM;v2;rg;vm" : null,
            itemScope ? "Azure" : null, "tenant", Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync(Arguments + " --tenant tenant" +
            (itemScope ? " --container IaasVMContainer;v2;rg;vm --protected-item VM;v2;rg;vm --fabric Azure" : ""));
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.OperationGetCommandResult);
        Assert.Equal(expected.OperationId, result.Operation.OperationId);
        Assert.Equal(expected.Scope, result.Operation.Scope);
        Assert.Equal(expected.StartTime, result.Operation.StartTime);
        Assert.Equal("job-one", result.Operation.JobId);
        Assert.Equal("job-two", Assert.Single(result.Operation.JobIds));
    }

    [Theory]
    [InlineData("--subscription sub --vault vault --resource-group rg")]
    [InlineData("--operation op --vault vault --resource-group rg")]
    [InlineData("--operation op --subscription sub --resource-group rg")]
    [InlineData("--operation op --subscription sub --vault vault")]
    [InlineData(Arguments + " --container c")]
    [InlineData(Arguments + " --protected-item p")]
    [InlineData(Arguments + " --fabric Azure")]
    [InlineData(Arguments + " --vault-type dpp")]
    public async Task ExecuteAsync_RejectsMissingRequiredAndInvalidScope(string args)
    {
        var response = await ExecuteCommandAsync(args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--operation", "../other")]
    [InlineData("--operation", "https://example.org/token")]
    [InlineData("--operation", "token?api-version=other")]
    [InlineData("--operation", "token#fragment")]
    [InlineData("--operation", "%252fother")]
    [InlineData("--operation", "..")]
    [InlineData("--operation", "x\\y")]
    [InlineData("--operation", "x\ny")]
    [InlineData("--fabric", "../Azure")]
    [InlineData("--fabric", "Azure?other=1")]
    [InlineData("--fabric", "%2fAzure")]
    [InlineData("--container", "c/p")]
    [InlineData("--protected-item", "p#x")]
    [InlineData("--vault", "a/b")]
    [InlineData("--vault", "1vault")]
    [InlineData("--vault", "v")]
    [InlineData("--resource-group", "rg.")]
    [InlineData("--resource-group", "rg?x")]
    public async Task ExecuteAsync_RejectsPathInjectionBeforeCallingService(string option, string value)
    {
        Dictionary<string, string> args = new()
        {
            ["--subscription"] = "sub",
            ["--vault"] = "vault",
            ["--resource-group"] = "rg",
            ["--operation"] = "op",
            ["--container"] = "c",
            ["--protected-item"] = "p",
            ["--fabric"] = "Azure"
        };
        args[option] = value;
        var response = await ExecuteCommandAsync(args.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(500)]
    public async Task ExecuteAsync_RedactsServiceErrorsIncludingExceptionResult(int status)
    {
        Service.GetOperationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, "SECRET-BODY credential=secret"));
        var response = await ExecuteCommandAsync(Arguments);
        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.DoesNotContain("SECRET-BODY", response.Message);
        Assert.DoesNotContain("SECRET-BODY", response.Results!.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_AllowsNoJobWithoutSubstitutingOperationId()
    {
        Service.GetOperationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new BackupOperationInfo("op", "/operation", "vault", "rsv", "InProgress", null, null, null, null, [], new Dictionary<string, string>()));
        var response = await ExecuteCommandAsync(Arguments);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.OperationGetCommandResult);
        Assert.Null(result.Operation.JobId);
        Assert.Empty(result.Operation.JobIds);
    }
}

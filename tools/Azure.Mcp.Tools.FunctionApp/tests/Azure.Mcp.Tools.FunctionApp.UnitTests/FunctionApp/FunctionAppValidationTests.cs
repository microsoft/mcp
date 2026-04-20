// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Services;
using Azure.ResourceManager.AppService.Models;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.UnitTests.FunctionApp;

public sealed class FunctionAppValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseStorageAuthMode_UnspecifiedReturnsNull(string? mode)
    {
        Assert.Null(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("managed-identity")]
    [InlineData("managedidentity")]
    [InlineData("mi")]
    [InlineData("MANAGED-IDENTITY")]
    [InlineData("  Managed-Identity  ")]
    public void ParseStorageAuthMode_ManagedIdentityAliasesReturnTrue(string mode)
    {
        Assert.True(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("connection-string")]
    [InlineData("connectionstring")]
    [InlineData("key")]
    [InlineData("Connection-String")]
    public void ParseStorageAuthMode_ConnectionStringAliasesReturnFalse(string mode)
    {
        Assert.False(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("oauth")]
    [InlineData("none")]
    public void ParseStorageAuthMode_UnknownValuesThrow(string mode)
    {
        var ex = Assert.Throws<ArgumentException>(() => FunctionAppValidation.ParseStorageAuthMode(mode));
        Assert.Contains("--storage-auth-mode", ex.Message);
    }

    [Theory]
    [InlineData(null, HostingKind.Consumption)]
    [InlineData("flex", HostingKind.FlexConsumption)]
    [InlineData("premium", HostingKind.Premium)]
    [InlineData("appservice", HostingKind.AppService)]
    [InlineData("containerapp", HostingKind.ContainerApp)]
    public void BuildCreateOptions_ManagedIdentityPropagatesAcrossHostingKinds(string? planType, HostingKind expected)
    {
        var inputs = BuildInputs(planType);
        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentityStorage: true);
        Assert.Equal(expected, options.HostingKind);
        Assert.True(options.UseManagedIdentityStorage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("flex")]
    [InlineData("premium")]
    [InlineData("appservice")]
    [InlineData("containerapp")]
    public void BuildCreateOptions_ConnectionStringPropagatesAcrossHostingKinds(string? planType)
    {
        var inputs = BuildInputs(planType);
        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentityStorage: false);
        Assert.False(options.UseManagedIdentityStorage);
    }

    [Fact]
    public void BuildCreateOptions_DefaultIsConnectionString()
    {
        var options = FunctionAppValidation.BuildCreateOptions(BuildInputs(null));
        Assert.False(options.UseManagedIdentityStorage);
    }

    [Fact]
    public void BuildDeploymentStorageForAccount_ManagedIdentityEmitsSystemAssignedAuth()
    {
        var storage = FunctionAppStorageProvisioner.BuildDeploymentStorageForAccount("mystorage123", useManagedIdentity: true);

        Assert.NotNull(storage);
        Assert.Equal(FunctionAppStorageType.BlobContainer, storage!.StorageType);
        Assert.Equal(new Uri("https://mystorage123.blob.core.windows.net/azure-webjobs-hosts"), storage.Value);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.SystemAssignedIdentity, storage.Authentication!.AuthenticationType);
        Assert.Null(storage.Authentication.StorageAccountConnectionStringName);
    }

    [Fact]
    public void BuildDeploymentStorageForAccount_ConnectionStringEmitsConnectionStringAuth()
    {
        var storage = FunctionAppStorageProvisioner.BuildDeploymentStorageForAccount("mystorage123", useManagedIdentity: false);

        Assert.NotNull(storage);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.StorageAccountConnectionString, storage!.Authentication!.AuthenticationType);
        Assert.Equal("AzureWebJobsStorage", storage.Authentication.StorageAccountConnectionStringName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildDeploymentStorageForAccount_EmptyAccountReturnsNull(string? accountName)
    {
        Assert.Null(FunctionAppStorageProvisioner.BuildDeploymentStorageForAccount(accountName, useManagedIdentity: true));
        Assert.Null(FunctionAppStorageProvisioner.BuildDeploymentStorageForAccount(accountName, useManagedIdentity: false));
    }

    [Fact]
    public void BuildDeploymentStorage_LegacyOverloadParsesAccountFromConnectionString()
    {
        var connectionString = FunctionAppStorageProvisioner.BuildConnectionString("legacyacct", "dGVzdGtleQ==");
        var storage = FunctionAppStorageProvisioner.BuildDeploymentStorage(connectionString);

        Assert.NotNull(storage);
        Assert.Equal(new Uri("https://legacyacct.blob.core.windows.net/azure-webjobs-hosts"), storage!.Value);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.StorageAccountConnectionString, storage.Authentication!.AuthenticationType);
    }

    private static NormalizedInputs BuildInputs(string? planType) =>
        new(
            Runtime: "dotnet",
            RuntimeVersion: null,
            PlanType: planType,
            PlanSku: null,
            OperatingSystem: null,
            StorageAccountName: null,
            ContainerAppsEnvironmentName: null);
}

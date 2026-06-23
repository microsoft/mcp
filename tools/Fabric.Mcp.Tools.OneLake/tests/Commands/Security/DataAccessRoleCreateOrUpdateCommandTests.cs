// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.Security;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Security;

public class DataAccessRoleCreateOrUpdateCommandTests : CommandUnitTestsBase<DataAccessRoleCreateOrUpdateCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create_or_update_data_access_role", Command.Name);
        Assert.Equal("Create or Update OneLake Data Access Role", Command.Title);
        Assert.Contains("Upsert a single data access role", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("create_or_update_data_access_role", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleCreateOrUpdateCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleCreateOrUpdateCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    private const string ValidRoleJson = "{\"name\":\"TestRole\",\"decisionRules\":[{\"effect\":\"Permit\",\"permission\":[{\"attributeName\":\"Action\",\"attributeValueIncludedIn\":[\"Read\"]},{\"attributeName\":\"Path\",\"attributeValueIncludedIn\":[\"*\"]}]}],\"members\":{\"fabricItemMembers\":[],\"microsoftEntraMembers\":[]}}";

    [Theory]
    [InlineData("--workspace-id ws1 --item-id item1", true)]
    [InlineData("--item-id item1", false)]  // missing workspace
    [InlineData("--workspace-id ws1", false)]  // missing item
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var roleJson = ValidRoleJson;
        if (shouldSucceed)
        {
            Service.CreateOrUpdateDataAccessRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new DataAccessRole { Name = "TestRole" });
        }

        var fullArgs = string.IsNullOrWhiteSpace(args)
            ? $"--role-definition {roleJson}"
            : $"{args} --role-definition {roleJson}";

        var response = await ExecuteCommandAsync(fullArgs);

        Assert.NotNull(response);
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulUpsert_ReturnsRole()
    {
        var expected = new DataAccessRole
        {
            Name = "TestRole",
            DecisionRules = [new DecisionRule { Effect = "Permit" }]
        };

        Service.CreateOrUpdateDataAccessRoleAsync("ws1", "item1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--role-definition", ValidRoleJson);

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.DataAccessRole);
        Assert.Equal("TestRole", result.Name);
        await Service.Received(1).CreateOrUpdateDataAccessRoleAsync("ws1", "item1", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateDataAccessRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Bad request"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--role-definition", ValidRoleJson);

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}


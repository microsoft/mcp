using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Azure.Mcp.Tools.Dps.Commands.EnrollmentGroup;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Options.EnrollmentGroup;
using Azure.Mcp.Tools.Dps.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Azure.Mcp.Tools.Dps.UnitTests.EnrollmentGroup;

public class EnrollmentGroupCreateCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = new EnrollmentGroupCreateCommand();
        Assert.Equal("create", command.Name);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Destructive);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var command = new EnrollmentGroupCreateCommand();
        var root = new System.CommandLine.RootCommand();
        command.RegisterOptions(root);

        var args = new[]
        {
            "--hostname", "test-dps.azure-devices-provisioning.net",
            "--enrollmentGroupId", "testgroup",
            "--attestationType", "symmetricKey",
            "--primaryKey", "abc123",
            "--secondaryKey", "def456",
            "--provisioningStatus", "enabled"
        };
        var parser = new Parser(root);
        var result = parser.Parse(args);

        var options = command.BindOptions(result);

        Assert.Equal("test-dps.azure-devices-provisioning.net", options.DpsHostName);
        Assert.Equal("testgroup", options.EnrollmentGroupId);
        Assert.Equal("symmetricKey", options.AttestationType);
        Assert.Equal("abc123", options.PrimaryKey);
        Assert.Equal("def456", options.SecondaryKey);
        Assert.Equal("enabled", options.ProvisioningStatus);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesEnrollmentGroup()
    {
        var mockService = new Mock<IEnrollmentGroupService>();
        var expectedGroup = new EnrollmentGroup
        {
            EnrollmentGroupId = "testgroup"
        };
        mockService.Setup(s => s.CreateOrUpdateAsync(
            It.IsAny<string>(),
            It.IsAny<EnrollmentGroup>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGroup);

        var services = new ServiceCollection();
        services.AddSingleton(mockService.Object);
        var provider = services.BuildServiceProvider();

        var command = new EnrollmentGroupCreateCommand();
        var context = new CommandContext(provider);

        var root = new System.CommandLine.RootCommand();
        command.RegisterOptions(root);
        var args = new[]
        {
            "--hostname", "test-dps.azure-devices-provisioning.net",
            "--enrollmentGroupId", "testgroup",
            "--attestationType", "symmetricKey"
        };
        var parser = new Parser(root);
        var parseResult = parser.Parse(args);

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.NotNull(response.Results);
        var resultObj = response.Results!.Value as EnrollmentGroupCreateCommand.EGCreateCommandResult;
        Assert.NotNull(resultObj);
        Assert.Equal("testgroup", resultObj!.EnrollmentGroup.EnrollmentGroupId);
    }
}

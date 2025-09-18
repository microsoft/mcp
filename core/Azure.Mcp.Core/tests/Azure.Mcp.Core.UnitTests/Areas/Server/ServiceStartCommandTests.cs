// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class ServiceStartCommandTests
{
    private readonly ServiceStartCommand _command;

    public ServiceStartCommandTests()
    {
        _command = new();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act

        // Assert
        Assert.Equal("start", _command.GetCommand().Name);
        Assert.Equal("Starts Azure MCP Server.", _command.GetCommand().Description!);
    }

    [Theory]
    [InlineData(null, "", "stdio")]
    [InlineData("storage", "storage", "stdio")]
    public void ServiceOption_ParsesCorrectly(string? inputService, string expectedService, string expectedTransport)
    {
        // Arrange
        var parseResult = CreateParseResult(inputService);

        // Act
        var actualServiceArray = parseResult.GetValue(ServiceOptionDefinitions.Namespace);
        var actualService = (actualServiceArray != null && actualServiceArray.Length > 0) ? actualServiceArray[0] : "";
        var actualTransport = parseResult.GetValue(ServiceOptionDefinitions.Transport);

        // Assert
        Assert.Equal(expectedService, actualService ?? "");
        Assert.Equal(expectedTransport, actualTransport);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InsecureDisableElicitationOption_ParsesCorrectly(bool expectedValue)
    {
        // Arrange
        var parseResult = CreateParseResultWithInsecureDisableElicitation(expectedValue);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.InsecureDisableElicitation);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void InsecureDisableElicitationOption_DefaultsToFalse()
    {
        // Arrange
        var parseResult = CreateParseResult(null);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.InsecureDisableElicitation);

        // Assert
        Assert.False(actualValue);
    }

    [Fact]
    public void AllOptionsRegistered_IncludesInsecureDisableElicitation()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasInsecureDisableElicitationOption = command.Options.Any(o =>
            o.Name == ServiceOptionDefinitions.InsecureDisableElicitation.Name);
        Assert.True(hasInsecureDisableElicitationOption, "InsecureDisableElicitation option should be registered");
    }

    [Theory]
    [InlineData("sse")]
    [InlineData("websocket")]
    [InlineData("http")]
    [InlineData("invalid")]
    public async Task ExecuteAsync_InvalidTransport_ThrowsArgumentException(string invalidTransport)
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport(invalidTransport);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _command.ExecuteAsync(context, parseResult));

        Assert.Contains($"Invalid transport '{invalidTransport}'", exception.Message);
        Assert.Contains("Valid transports are: stdio", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidTransport_DoesNotThrow()
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport("stdio");
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act & Assert - This will throw because the server can't actually start in a unit test,
        // but it should not throw an ArgumentException about invalid transport
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => _command.ExecuteAsync(context, parseResult));

        Assert.IsNotType<ArgumentException>(exception);
    }

    private static ParseResult CreateParseResult(string? serviceValue)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport
        };
        var args = new List<string>();
        if (!string.IsNullOrEmpty(serviceValue))
        {
            args.Add("--namespace");
            args.Add(serviceValue);
        }
        // Add required transport default for test
        args.Add("--transport");
        args.Add("stdio");

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithInsecureDisableElicitation(bool insecureDisableElicitation)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport",
            "stdio"
        };

        if (insecureDisableElicitation)
        {
            args.Add("--insecure-disable-elicitation");
        }

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithTransport(string transport)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport",
            transport,
            "--mode",
            "all",
            "--read-only"
        };

        return root.Parse([.. args]);
    }
}

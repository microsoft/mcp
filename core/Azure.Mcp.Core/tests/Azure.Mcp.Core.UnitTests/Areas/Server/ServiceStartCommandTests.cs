// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Options;
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
}

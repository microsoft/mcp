// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
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
        var actualServiceArray = parseResult.GetValueForOption(ServiceOptionDefinitions.Namespace);
        var actualService = (actualServiceArray != null && actualServiceArray.Length > 0) ? actualServiceArray[0] : "";
        var actualTransport = parseResult.GetValueForOption(ServiceOptionDefinitions.Transport);

        // Assert
        Assert.Equal(expectedService, actualService ?? "");
        Assert.Equal(expectedTransport, actualTransport);
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
        return new Parser(root).Parse(args.ToArray());
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class EnumOptionSchemaTests()
{
    private static readonly string[] s_days =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    [Fact]
    public void CreateInputSchema_EnumNamesSurviveStringBackedBinding()
    {
        var command = CreateCommand();
        var schema = OptionSchemaGenerator.CreateInputSchema(command.Options.ToArray());
        var properties = schema["properties"]!;

        foreach (var name in new[] { "day", "optional-day" })
        {
            Assert.Equal("string", properties[name]!["type"]!.GetValue<string>());
            Assert.Equal(s_days, properties[name]!["enum"]!.AsArray().Select(value => value!.GetValue<string>()));
        }

        Assert.Equal(["day", "days"], schema["required"]!.AsArray().Select(value => value!.GetValue<string>()));
        Assert.False(schema["additionalProperties"]!.GetValue<bool>());
    }

    [Fact]
    public void CreateInputSchema_EnumArraysConstrainItemsNotTheArray()
    {
        var schema = OptionSchemaGenerator.CreateInputSchema(CreateCommand().Options.ToArray());
        foreach (var name in new[] { "days", "optional-days" })
        {
            var property = schema["properties"]![name]!;
            Assert.Equal("array", property["type"]!.GetValue<string>());
            Assert.Null(property["enum"]);
            Assert.Equal("string", property["items"]!["type"]!.GetValue<string>());
            Assert.Equal(s_days, property["items"]!["enum"]!.AsArray().Select(value => value!.GetValue<string>()));
        }
    }

    [Fact]
    public void CreateInputSchema_CompletionsDoNotConstrainOrdinaryStrings()
    {
        var command = CreateCommand();
        command.Options.Single(option => option.Name == "--text").CompletionSources.Add("example");

        var schema = OptionSchemaGenerator.CreateInputSchema(command.Options.ToArray());

        Assert.Null(schema["properties"]!["text"]!["enum"]);
    }

    [Fact]
    public void ToOptionInfo_SerializesScalarAndElementEnumMetadata()
    {
        var options = CreateCommand().Options.ToDictionary(option => option.Name);
        var scalar = options["--day"].ToOptionInfo();
        var array = options["--days"].ToOptionInfo();
        var text = options["--text"].ToOptionInfo();

        Assert.Equal(s_days, scalar.EnumValues);
        Assert.Null(scalar.ElementEnumValues);
        Assert.Null(array.EnumValues);
        Assert.Equal(s_days, array.ElementEnumValues);

        var scalarJson = JsonSerializer.SerializeToElement(scalar, ModelsJsonContext.Default.OptionInfo);
        var arrayJson = JsonSerializer.SerializeToElement(array, ModelsJsonContext.Default.OptionInfo);
        var textJson = JsonSerializer.SerializeToElement(text, ModelsJsonContext.Default.OptionInfo);
        Assert.True(scalarJson.TryGetProperty("enum", out _));
        Assert.False(scalarJson.TryGetProperty("elementEnum", out _));
        Assert.True(arrayJson.TryGetProperty("elementEnum", out _));
        Assert.False(arrayJson.TryGetProperty("enum", out _));
        Assert.False(textJson.TryGetProperty("enum", out _));
        Assert.False(textJson.TryGetProperty("elementEnum", out _));
    }

    [Fact]
    public void BindOptions_StillAcceptsCaseInsensitiveNamesAndPreservesOmission()
    {
        var command = CreateCommand();
        var options = OptionBinder.BindOptions<EnumSchemaOptions>(
            command.Parse("--day mOnDaY --days tuesday FRIDAY"));

        Assert.Equal(DayOfWeek.Monday, options.Day);
        Assert.Equal([DayOfWeek.Tuesday, DayOfWeek.Friday], options.Days);
        Assert.Null(options.OptionalDay);
        Assert.Null(options.OptionalDays);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("999")]
    [InlineData("Monday,Friday")]
    [InlineData("InvalidDay")]
    [InlineData("")]
    [InlineData(" ")]
    public void BindOptions_RejectsValuesOutsideTheEnum(string value)
    {
        var command = CreateCommand();
        var parseResult = command.Parse(["--day", value, "--days", "Tuesday"]);

        var exception = Assert.Throws<CommandValidationException>(
            () => OptionBinder.BindOptions<EnumSchemaOptions>(parseResult));

        Assert.Contains("--day", exception.Message);
        Assert.Contains("Must be one of", exception.Message);
    }

    [Fact]
    public void BindOptions_RejectsInvalidEnumArrayElements()
    {
        var command = CreateCommand();
        var parseResult = command.Parse("--day Monday --days Tuesday InvalidDay");

        var exception = Assert.Throws<CommandValidationException>(
            () => OptionBinder.BindOptions<EnumSchemaOptions>(parseResult));

        Assert.Contains("--days", exception.Message);
        Assert.Contains("InvalidDay", exception.Message);
    }

    private static Command CreateCommand()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EnumSchemaOptions>(command);
        return command;
    }
}

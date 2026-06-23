// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class OptionBinderTests
{
    #region Test Options Classes

    private sealed class StringOptions
    {
        [Option(Description = "The name of the item.")]
        public string? Name { get; set; }
        [Option(Description = "The description of the item.")]
        public string? Description { get; set; }
    }

    private sealed class IntOptions
    {
        [Option(Description = "The number of items.")]
        public int Count { get; set; }
        [Option(Description = "The maximum number of items.")]
        public int? Limit { get; set; }
    }

    private sealed class BoolOptions
    {
        [Option(Description = "Whether to display verbose output.")]
        public bool Verbose { get; set; }
        [Option(Description = "Whether to enable debug mode.")]
        public bool? Debug { get; set; }
    }

    private sealed class ArrayOptions
    {
        [Option(Description = "The tags associated with the item.")]
        public required string[] Tags { get; set; }
        [Option(Description = "The ports associated with the item.")]
        public required int[] Ports { get; set; }
        [Option(Description = "The tags associated with the item.")]
        public string[]? NullableTags { get; set; }
        [Option(Description = "The ports associated with the item.")]
        public int[]? NullablePorts { get; set; }
    }

    private enum Color
    {
        Red,
        Green,
        Blue,
    }

    private sealed class EnumOptions
    {
        [Option(Description = "The color of the item.")]
        public Color Color { get; set; }
        [Option(Description = "The background color of the item.")]
        public Color? Background { get; set; }
    }

    private sealed class ArrayEnumOptions
    {
        [Option(Description = "The colors of the item.")]
        public required Color[] Colors { get; set; }
        [Option(Description = "The background colors of the item.")]
        public Color[]? Backgrounds { get; set; }
    }

    private sealed class GuidOptions
    {
        [Option(Description = "The ID of the item.")]
        public Guid Id { get; set; }
        [Option(Description = "The correlation ID of the item.")]
        public Guid? CorrelationId { get; set; }
    }

    private sealed class DateTimeOptions
    {
        [Option(Description = "The start date of the item.")]
        public DateTime StartDate { get; set; }
        [Option(Description = "The timestamp of the item.")]
        public DateTimeOffset? Timestamp { get; set; }
    }

    private sealed class DecimalOptions
    {
        [Option(Description = "The price of the item.")]
        public decimal Price { get; set; }
        [Option(Description = "The rate of the item.")]
        public double? Rate { get; set; }
    }

    private sealed class UnsupportedTypeOptions
    {
        [Option(Description = "The value of the item.")]
        public object[]? Value { get; set; }
    }

    private sealed class NetworkSettings
    {
        [Option(Description = "The host of the item.")]
        public string? Host { get; set; }
        [Option(Description = "The port of the item.")]
        public int? Port { get; set; }
    }

    private sealed class RequiredNetworkSettings
    {
        [Option(Description = "The name of the item.")]
        public required string Name { get; set; }
        [Option(Description = "The port of the item.")]
        public int? Port { get; set; }
    }

    private sealed class NestedOptional
    {
        [Option(Description = "The name of the item.")]
        public string? Name { get; set; }
        [OptionContainer]
        public NetworkSettings? Optional { get; set; }
        [OptionContainer]
        public required NetworkSettings Required { get; set; }
    }

    private sealed class NestedRequired
    {
        [Option(Description = "The name of the item.")]
        public string? Name { get; set; }
        [OptionContainer]
        public required RequiredNetworkSettings Required { get; set; }
    }

    private sealed class AliasedOptions
    {
        [Option(Description = "The name of the item.", Name = "name", Aliases = ["n", "nm"])]
        public required string Name { get; set; }
        [Option(Description = "The address of the item.", Name = "address", Aliases = ["a", "addr"])]
        public string? Address { get; set; }
    }

    private sealed class DefaultedOptions
    {
        [Option(Description = "The default name.", DefaultValue = "default")]
        public string? Name { get; set; }
        [Option(Description = "The default count.", DefaultValue = 42)]
        public int? Count { get; set; }
    }

    private sealed class EmptyOrWhiteSpaceOptions
    {
        [Option(Description = "The name of the item.")]
        public required string Name { get; set; }
        [Option(Description = "The description of the item.", AllowEmptyOrWhiteSpaceString = true)]
        public required string Description { get; set; }
        [Option(Description = "The tags of the item.")]
        public required string[] Tags { get; set; }
        [Option(Description = "The notes of the item.", AllowEmptyOrWhiteSpaceString = true)]
        public required string[] Notes { get; set; }
    }

    private sealed class InvalidAttributesCombinationOptions
    {
        [Option(Description = "Invalid attribute combination.")]
        [OptionContainer]
        public NetworkSettings? Invalid { get; set; }
    }

    private sealed class InvalidOptionAttributeOptions
    {
        [Option(Description = "Invalid option attribute.")]
        public NetworkSettings? Invalid { get; set; }
    }

    private sealed class InvalidOptionContainerAttributeOptions
    {
        [OptionContainer]
        public string? Invalid { get; set; }
    }

    #endregion

    #region RegisterOptions Tests

    [Fact]
    public void RegisterOptions_String_RegistersCorrectOptions()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<StringOptions>(command);

        Assert.Equal(2, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--name");
        Assert.Contains(command.Options, o => o.Name == "--description");
    }

    [Fact]
    public void RegisterOptions_Int_RegistersCorrectOptions()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<IntOptions>(command);

        Assert.Equal(2, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--count");
        Assert.Contains(command.Options, o => o.Name == "--limit");
    }

    [Fact]
    public void RegisterOptions_Array_SetsExpectedArity()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<ArrayOptions>(command);

        var tagsOption = command.Options.Single(o => o.Name == "--tags");
        Assert.Equal(ArgumentArity.OneOrMore, tagsOption.Arity);
        Assert.True(tagsOption.AllowMultipleArgumentsPerToken);

        var nullableTagsOption = command.Options.Single(o => o.Name == "--nullable-tags");
        Assert.Equal(ArgumentArity.ZeroOrMore, nullableTagsOption.Arity);
        Assert.True(nullableTagsOption.AllowMultipleArgumentsPerToken);
    }

    [Fact]
    public void RegisterOptions_Enum_RegistersAsStringOption()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<EnumOptions>(command);

        Assert.Equal(2, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--color");
        Assert.Contains(command.Options, o => o.Name == "--background");
        Assert.Equal(typeof(string), command.Options.Single(o => o.Name == "--color").ValueType);
        Assert.Equal(typeof(string), command.Options.Single(o => o.Name == "--background").ValueType);
    }

    [Fact]
    public void RegisterOptions_ArrayEnum_RegistersAsStringArrayOption()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<ArrayEnumOptions>(command);

        Assert.Equal(2, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--colors");
        Assert.Contains(command.Options, o => o.Name == "--backgrounds");
        Assert.Equal(typeof(string[]), command.Options.Single(o => o.Name == "--colors").ValueType);
        Assert.Equal(typeof(string[]), command.Options.Single(o => o.Name == "--backgrounds").ValueType);
    }

    [Fact]
    public void RegisterOptions_UnsupportedType_Throws()
    {
        var command = new Command("test");

        var ex = Assert.Throws<InvalidOperationException>(
            () => OptionBinder.RegisterOptions<UnsupportedTypeOptions>(command));

        Assert.Contains("non-scalar element type", ex.Message);
    }

    [Fact]
    public void RegisterOptions_InvalidAttributes_Throws()
    {
        var command = new Command("test");

        var ex = Assert.Throws<InvalidOperationException>(
            () => OptionBinder.RegisterOptions<InvalidAttributesCombinationOptions>(command));

        Assert.Contains("Properties can only be attributed with [Option] or [OptionContainer], not both.", ex.Message);
    }

    [Fact]
    public void RegisterOptions_InvalidOptionAttribute_Throws()
    {
        var command = new Command("test");

        var ex = Assert.Throws<InvalidOperationException>(
            () => OptionBinder.RegisterOptions<InvalidOptionAttributeOptions>(command));

        Assert.Contains("Complex properties cannot use [Option] attribute. Use [OptionContainer] instead.", ex.Message);
    }

    [Fact]
    public void RegisterOptions_InvalidOptionContainerAttribute_Throws()
    {
        var command = new Command("test");

        var ex = Assert.Throws<InvalidOperationException>(
            () => OptionBinder.RegisterOptions<InvalidOptionContainerAttributeOptions>(command));

        Assert.Contains("Non-complex properties cannot use [OptionContainer] attribute. Use [Option] instead.", ex.Message);
    }

    #endregion

    #region BindOptions Tests

    [Fact]
    public void BindOptions_String_BindsValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<StringOptions>(command);

        var parseResult = command.Parse("--name hello --description world");
        var options = OptionBinder.BindOptions<StringOptions>(parseResult);

        Assert.Equal("hello", options.Name);
        Assert.Equal("world", options.Description);
    }

    [Fact]
    public void BindOptions_String_NullWhenNotProvided()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<StringOptions>(command);

        var parseResult = command.Parse("");
        var options = OptionBinder.BindOptions<StringOptions>(parseResult);

        Assert.Null(options.Name);
        Assert.Null(options.Description);
    }

    [Fact]
    public void BindOptions_Int_BindsValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<IntOptions>(command);

        var parseResult = command.Parse("--count 42 --limit 100");
        var options = OptionBinder.BindOptions<IntOptions>(parseResult);

        Assert.Equal(42, options.Count);
        Assert.Equal(100, options.Limit);
    }

    [Fact]
    public void BindOptions_Bool_BindsValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<BoolOptions>(command);

        var parseResult = command.Parse("--verbose true --debug false");
        var options = OptionBinder.BindOptions<BoolOptions>(parseResult);

        Assert.True(options.Verbose);
        Assert.Equal(false, options.Debug);
    }

    [Fact]
    public void BindOptions_Array_BindsMultipleValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<ArrayOptions>(command);

        var parseResult = command.Parse("--tags foo bar baz --ports 80 443");
        var options = OptionBinder.BindOptions<ArrayOptions>(parseResult);

        Assert.Equal(["foo", "bar", "baz"], options.Tags);
        Assert.Equal([80, 443], options.Ports);
        Assert.Null(options.NullableTags);
        Assert.Null(options.NullablePorts);
    }

    [Fact]
    public void BindOptions_Enum_BindsCaseInsensitive()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EnumOptions>(command);

        var parseResult = command.Parse("--color green --background BLUE");
        var options = OptionBinder.BindOptions<EnumOptions>(parseResult);

        Assert.Equal(Color.Green, options.Color);
        Assert.Equal(Color.Blue, options.Background);
    }

    [Fact]
    public void BindOptions_Enum_MixedCase()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EnumOptions>(command);

        var parseResult = command.Parse("--color ReD");
        var options = OptionBinder.BindOptions<EnumOptions>(parseResult);

        Assert.Equal(Color.Red, options.Color);
    }

    [Fact]
    public void BindOptions_NullableEnum_NullWhenNotProvided()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EnumOptions>(command);

        var parseResult = command.Parse("--color Red");
        var options = OptionBinder.BindOptions<EnumOptions>(parseResult);

        Assert.Equal(Color.Red, options.Color);
        Assert.Null(options.Background);
    }

    [Fact]
    public void BindOptions_ArrayEnum_BindsCaseInsensitive()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<ArrayEnumOptions>(command);

        var parseResult = command.Parse("--colors green red --backgrounds BLUE GREEN");
        var options = OptionBinder.BindOptions<ArrayEnumOptions>(parseResult);

        Assert.Equal(2, options.Colors.Length);
        Assert.Contains(Color.Green, options.Colors);
        Assert.Contains(Color.Red, options.Colors);

        Assert.NotNull(options.Backgrounds);
        Assert.Equal(2, options.Backgrounds.Length);
        Assert.Contains(Color.Blue, options.Backgrounds);
        Assert.Contains(Color.Green, options.Backgrounds);
    }

    [Fact]
    public void BindOptions_ArrayEnum_MixedCase()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<ArrayEnumOptions>(command);

        var parseResult = command.Parse("--colors gREEn ReD");
        var options = OptionBinder.BindOptions<ArrayEnumOptions>(parseResult);

        Assert.Equal(2, options.Colors.Length);
        Assert.Contains(Color.Green, options.Colors);
        Assert.Contains(Color.Red, options.Colors);
    }

    [Fact]
    public void BindOptions_NullableArrayEnum_NullWhenNotProvided()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<ArrayEnumOptions>(command);

        var parseResult = command.Parse("--colors gREEn Red");
        var options = OptionBinder.BindOptions<ArrayEnumOptions>(parseResult);

        Assert.Equal(2, options.Colors.Length);
        Assert.Contains(Color.Green, options.Colors);
        Assert.Contains(Color.Red, options.Colors);

        Assert.Null(options.Backgrounds);
    }

    [Fact]
    public void BindOptions_InvalidEnumValue_Throws()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EnumOptions>(command);

        var parseResult = command.Parse("--color Invalid");
        var ex = Assert.Throws<CommandValidationException>(() => OptionBinder.BindOptions<EnumOptions>(parseResult));

        Assert.Contains("Argument 'Invalid' not recognized. Must be one of:", ex.Message);
    }

    [Fact]
    public void BindOptions_Guid_BindsValue()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<GuidOptions>(command);
        var guid = Guid.NewGuid();

        var parseResult = command.Parse($"--id {guid}");
        var options = OptionBinder.BindOptions<GuidOptions>(parseResult);

        Assert.Equal(guid, options.Id);
        Assert.Null(options.CorrelationId);
    }

    [Fact]
    public void BindOptions_DateTime_BindsValue()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<DateTimeOptions>(command);

        var parseResult = command.Parse("--start-date 2024-01-15");
        var options = OptionBinder.BindOptions<DateTimeOptions>(parseResult);

        Assert.Equal(new DateTime(2024, 1, 15), options.StartDate);
    }


    [Fact]
    public void BindOptions_Decimal_BindsValue()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<DecimalOptions>(command);

        var parseResult = command.Parse("--price 19.99 --rate 3.14");
        var options = OptionBinder.BindOptions<DecimalOptions>(parseResult);

        Assert.Equal(19.99m, options.Price);
        Assert.Equal(3.14, options.Rate);
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("n", null)]
    [InlineData("nm", null)]
    [InlineData("name", "address")]
    [InlineData("n", "a")]
    [InlineData("nm", "addr")]
    public void BindOptions_Aliases_BindsValue(string nameArgName, string? addressArgName)
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<AliasedOptions>(command);

        var args = new List<string> { $"--{nameArgName}", "John" };
        if (addressArgName != null)
        {
            args.AddRange([$"--{addressArgName}", "Seattle"]);
        }

        var parseResult = command.Parse(args);
        var options = OptionBinder.BindOptions<AliasedOptions>(parseResult);

        Assert.Equal("John", options.Name);
        if (addressArgName != null)
        {
            Assert.Equal("Seattle", options.Address);
        }
    }

    [Fact]
    public void BindOptions_DefaultValue_BindsDefault()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<DefaultedOptions>(command);

        var parseResult = command.Parse("");
        var options = OptionBinder.BindOptions<DefaultedOptions>(parseResult);

        Assert.Equal("default", options.Name);
        Assert.Equal(42, options.Count);
    }

    #endregion

    #region Nested Options Tests

    [Fact]
    public void RegisterOptions_NestedOptional_FlattenedWithPrefix()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<NestedOptional>(command);

        // --name, --optional-host, --optional-port, --required-host, --required-port
        Assert.Equal(5, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--name");
        Assert.Contains(command.Options, o => o.Name == "--optional-host");
        Assert.Contains(command.Options, o => o.Name == "--optional-port");
        Assert.Contains(command.Options, o => o.Name == "--required-host");
        Assert.Contains(command.Options, o => o.Name == "--required-port");
    }

    [Fact]
    public void RegisterOptions_NestedOptional_AllChildrenAreOptional()
    {
        // NetworkSettings has all-nullable children, so whether parent is nullable or not,
        // the leaf options remain optional
        var command = new Command("test");

        OptionBinder.RegisterOptions<NestedOptional>(command);

        foreach (var option in command.Options)
        {
            Assert.False(option.Required, $"Option {option.Name} should be optional");
        }
    }

    [Fact]
    public void BindOptions_NestedOptional_NullWhenNoChildProvided()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<NestedOptional>(command);

        var parseResult = command.Parse("--name myapp");
        var options = OptionBinder.BindOptions<NestedOptional>(parseResult);

        Assert.Equal("myapp", options.Name);
        Assert.Null(options.Optional);
    }

    [Fact]
    public void BindOptions_NestedOptional_BindsOptionalChild()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<NestedOptional>(command);

        var parseResult = command.Parse("--optional-host localhost --optional-port 8080");
        var options = OptionBinder.BindOptions<NestedOptional>(parseResult);

        Assert.NotNull(options.Optional);
        Assert.Equal("localhost", options.Optional!.Host);
        Assert.Equal(8080, options.Optional.Port);
    }

    [Fact]
    public void BindOptions_NestedOptional_BindsRequiredChild()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<NestedOptional>(command);

        var parseResult = command.Parse("--required-host 10.0.0.1 --required-port 443");
        var options = OptionBinder.BindOptions<NestedOptional>(parseResult);

        Assert.NotNull(options.Required);
        Assert.Equal("10.0.0.1", options.Required.Host);
        Assert.Equal(443, options.Required.Port);
    }

    [Fact]
    public void BindOptions_NestedOptional_PartialChildValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<NestedOptional>(command);

        var parseResult = command.Parse("--optional-host localhost");
        var options = OptionBinder.BindOptions<NestedOptional>(parseResult);

        Assert.NotNull(options.Optional);
        Assert.Equal("localhost", options.Optional!.Host);
        Assert.Null(options.Optional.Port);
    }

    [Fact]
    public void RegisterOptions_NestedRequired_DeepFlattenWithPrefix()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<NestedRequired>(command);

        // --name, --required-name (required!), --required-port (optional)
        Assert.Equal(3, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--name");
        Assert.Contains(command.Options, o => o.Name == "--required-name");
        Assert.Contains(command.Options, o => o.Name == "--required-port");
    }

    [Fact]
    public void RegisterOptions_NestedRequired_ScalarChildIsRequired()
    {
        // RequiredNetworkSettings.Name is non-nullable string → required
        var command = new Command("test");

        OptionBinder.RegisterOptions<NestedRequired>(command);

        var requiredName = command.Options.Single(o => o.Name == "--required-name");
        Assert.True(requiredName.Required);

        // Port is nullable → optional
        var port = command.Options.Single(o => o.Name == "--required-port");
        Assert.False(port.Required);
    }

    [Fact]
    public void BindOptions_NestedRequired_BindsDeepValues()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<NestedRequired>(command);

        var parseResult = command.Parse("--name myapp --required-name primary --required-port 5432");
        var options = OptionBinder.BindOptions<NestedRequired>(parseResult);

        Assert.Equal("myapp", options.Name);
        Assert.NotNull(options.Required);
        Assert.Equal("primary", options.Required.Name);
        Assert.Equal(5432, options.Required.Port);
    }

    [Fact]
    public void BindOptions_EmptyOrWhiteSpace_Allowed()
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EmptyOrWhiteSpaceOptions>(command);

        var parseResult = command.Parse("--name John --description \"   \" --tags updated --notes \"\"");
        var options = OptionBinder.BindOptions<EmptyOrWhiteSpaceOptions>(parseResult);

        Assert.Equal("   ", options.Description);
        Assert.Single(options.Notes);
        Assert.Equal("", options.Notes[0]);
    }

    [Theory]
    [InlineData("--name \"   \" --description description --tags updated --notes notes")]
    [InlineData("--name John --description description --tags \"\" --notes notes")]
    public void BindOptions_EmptyOrWhiteSpace_Rejected(string args)
    {
        var command = new Command("test");
        OptionBinder.RegisterOptions<EmptyOrWhiteSpaceOptions>(command);

        var parseResult = command.Parse(args);
        var exception = Assert.Throws<CommandValidationException>(() => OptionBinder.BindOptions<EmptyOrWhiteSpaceOptions>(parseResult));

        Assert.Contains("require non-empty, non-whitespace values", exception.Message);
    }

    #endregion

    #region Trimming Annotation Tests

    [Fact]
    public void OptionBinder_GenericMethods_PreservePublicPropertiesAndParameterlessConstructor()
    {
        var registerMethod = typeof(OptionBinder).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(OptionBinder.RegisterOptions) && m.IsGenericMethodDefinition);
        var bindMethod = typeof(OptionBinder).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(OptionBinder.BindOptions) && m.IsGenericMethodDefinition);

        AssertHasRequiredMemberAnnotations(registerMethod.GetGenericArguments()[0]);
        AssertHasRequiredMemberAnnotations(bindMethod.GetGenericArguments()[0]);
    }

    [Fact]
    public void TrimAnnotations_CommandAnnotations_IncludePublicPropertiesAndParameterlessConstructor()
    {
        var annotations = TrimAnnotations.CommandAnnotations;

        Assert.True(annotations.HasFlag(DynamicallyAccessedMemberTypes.PublicProperties));
        Assert.True(annotations.HasFlag(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor));
    }

    private static void AssertHasRequiredMemberAnnotations(MemberInfo member)
    {
        var attribute = member.GetCustomAttribute<DynamicallyAccessedMembersAttribute>();
        Assert.NotNull(attribute);
        Assert.True(attribute!.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.PublicProperties));
        Assert.True(attribute.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor));
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void GetHandler_Enum_ThreadSafe()
    {
        // Exercise concurrent access to enum handler caching
        var exceptions = new List<Exception>();

        Parallel.For(0, 100, _ =>
        {
            try
            {
                var command = new Command("test");
                OptionBinder.RegisterOptions<EnumOptions>(command);

                var parseResult = command.Parse("--color Green");
                var options = OptionBinder.BindOptions<EnumOptions>(parseResult);

                Assert.Equal(Color.Green, options.Color);
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        });

        Assert.Empty(exceptions);
    }

    #endregion
}

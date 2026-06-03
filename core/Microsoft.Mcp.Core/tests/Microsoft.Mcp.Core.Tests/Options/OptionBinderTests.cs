// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
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
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private sealed class IntOptions
    {
        public int Count { get; set; }
        public int? Limit { get; set; }
    }

    private sealed class BoolOptions
    {
        public bool Verbose { get; set; }
        public bool? Debug { get; set; }
    }

    private sealed class ArrayOptions
    {
        public string[]? Tags { get; set; }
        public int[]? Ports { get; set; }
    }

    private enum Color
    {
        Red,
        Green,
        Blue,
    }

    private sealed class EnumOptions
    {
        public Color Color { get; set; }
        public Color? Background { get; set; }
    }

    private sealed class GuidOptions
    {
        public Guid Id { get; set; }
        public Guid? CorrelationId { get; set; }
    }

    private sealed class DateTimeOptions
    {
        public DateTime StartDate { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }

    private sealed class DecimalOptions
    {
        public decimal Price { get; set; }
        public double? Rate { get; set; }
    }

    private sealed class UnsupportedTypeOptions
    {
        public object[]? Value { get; set; }
    }

    private sealed class NetworkSettings
    {
        public string? Host { get; set; }
        public int? Port { get; set; }
    }

    private sealed class RequiredNetworkSettings
    {
        public required string Name { get; set; }
        public int? Port { get; set; }
    }

    private sealed class NestedOptional
    {
        public string? Name { get; set; }
        public NetworkSettings? Optional { get; set; }
        public required NetworkSettings Required { get; set; }
    }

    private sealed class NestedRequired
    {
        public string? Name { get; set; }
        public required RequiredNetworkSettings Required { get; set; }
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
    public void RegisterOptions_Array_SetsArityToOneOrMore()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<ArrayOptions>(command);

        var tagsOption = command.Options.Single(o => o.Name == "--tags");
        Assert.Equal(ArgumentArity.OneOrMore, tagsOption.Arity);
        Assert.True(tagsOption.AllowMultipleArgumentsPerToken);
    }

    [Fact]
    public void RegisterOptions_Enum_RegistersAsStringOption()
    {
        var command = new Command("test");

        OptionBinder.RegisterOptions<EnumOptions>(command);

        Assert.Equal(2, command.Options.Count);
        Assert.Contains(command.Options, o => o.Name == "--color");
        Assert.Contains(command.Options, o => o.Name == "--background");
    }

    [Fact]
    public void RegisterOptions_UnsupportedType_Throws()
    {
        var command = new Command("test");

        var ex = Assert.Throws<InvalidOperationException>(
            () => OptionBinder.RegisterOptions<UnsupportedTypeOptions>(command));

        Assert.Contains("non-scalar element type", ex.Message);
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

        Assert.Equal(["foo", "bar", "baz"], options.Tags!);
        Assert.Equal([80, 443], options.Ports!);
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

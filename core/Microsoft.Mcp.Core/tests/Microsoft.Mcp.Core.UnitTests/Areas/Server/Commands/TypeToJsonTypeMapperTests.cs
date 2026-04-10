// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands;

/// <summary>
/// Tests for <see cref="TypeToJsonTypeMapper"/>, specifically the enum type mapping
/// that produces <c>"string"</c> type with named enum values for Codex compatibility.
/// </summary>
public sealed class TypeToJsonTypeMapperTests
{
    private enum TestColor
    {
        Red,
        Green,
        Blue
    }

    private enum TestStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }

    [Fact]
    public void ToJsonType_EnumType_ReturnsString()
    {
        // Act
        var jsonType = typeof(TestColor).ToJsonType();

        // Assert
        Assert.Equal("string", jsonType);
    }

    [Fact]
    public void ToJsonType_NullableEnumType_ReturnsString()
    {
        // Act
        var jsonType = typeof(TestColor?).ToJsonType();

        // Assert
        Assert.Equal("string", jsonType);
    }

    [Theory]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(int), "integer")]
    [InlineData(typeof(bool), "boolean")]
    [InlineData(typeof(double), "number")]
    [InlineData(typeof(decimal), "number")]
    public void ToJsonType_NonEnumTypes_ReturnsExpectedType(Type type, string expectedJsonType)
    {
        // Act
        var jsonType = type.ToJsonType();

        // Assert
        Assert.Equal(expectedJsonType, jsonType);
    }

    [Fact]
    public void ToJsonType_NullType_ReturnsNull()
    {
        // Act
        var jsonType = ((Type?)null).ToJsonType();

        // Assert
        Assert.Equal("null", jsonType);
    }

    [Fact]
    public void CreatePropertySchema_EnumType_ReturnsStringTypeWithEnumValues()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(TestColor), "Color selection");

        // Assert
        Assert.Equal("string", schema.Type);
        Assert.Equal("Color selection", schema.Description);
        Assert.NotNull(schema.Enum);
        Assert.Equal(["Red", "Green", "Blue"], schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_NullableEnumType_ReturnsStringTypeWithEnumValues()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(TestStatus?), "Status selection");

        // Assert
        Assert.Equal("string", schema.Type);
        Assert.NotNull(schema.Enum);
        Assert.Equal(["Pending", "Active", "Completed", "Cancelled"], schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_StringType_DoesNotIncludeEnum()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(string), "A string option");

        // Assert
        Assert.Equal("string", schema.Type);
        Assert.Null(schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_IntType_DoesNotIncludeEnum()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(int), "An integer option");

        // Assert
        Assert.Equal("integer", schema.Type);
        Assert.Null(schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_BoolType_DoesNotIncludeEnum()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(bool), "A boolean option");

        // Assert
        Assert.Equal("boolean", schema.Type);
        Assert.Null(schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_EnumType_NullDescription_LeavesDescriptionNull()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(TestColor), null);

        // Assert
        Assert.Equal("string", schema.Type);
        Assert.Null(schema.Description);
        Assert.NotNull(schema.Enum);
    }

    [Fact]
    public void CreatePropertySchema_NonEnumType_NullDescription_LeavesDescriptionNull()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(string), null);

        // Assert
        Assert.Equal("string", schema.Type);
        Assert.Null(schema.Description);
    }

    [Fact]
    public void CreatePropertySchema_NonEnumType_WithDescription_PreservesDescription()
    {
        // Act
        var schema = TypeToJsonTypeMapper.CreatePropertySchema(typeof(int), "An integer value");

        // Assert
        Assert.Equal("integer", schema.Type);
        Assert.Equal("An integer value", schema.Description);
    }
}

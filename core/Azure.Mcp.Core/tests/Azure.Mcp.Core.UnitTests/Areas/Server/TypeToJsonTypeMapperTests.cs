// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Azure.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class TypeToJsonTypeMapperTests
{
    [Fact]
    public void ToJsonType_WithNullType_ReturnsNull()
    {
        // Arrange
        Type? nullType = null;

        // Act
        var result = nullType.ToJsonType();

        // Assert
        Assert.Equal("null", result);
    }

    [Theory]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(char), "string")]
    [InlineData(typeof(Guid), "string")]
    [InlineData(typeof(DateTime), "string")]
    [InlineData(typeof(DateTimeOffset), "string")]
    [InlineData(typeof(TimeSpan), "string")]
    [InlineData(typeof(Uri), "string")]
    public void ToJsonType_WithStringTypes_ReturnsString(Type type, string expected)
    {
        // Act
        var result = type.ToJsonType();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(int), "integer")]
    [InlineData(typeof(uint), "integer")]
    [InlineData(typeof(long), "integer")]
    [InlineData(typeof(ulong), "integer")]
    [InlineData(typeof(short), "integer")]
    [InlineData(typeof(ushort), "integer")]
    [InlineData(typeof(byte), "integer")]
    [InlineData(typeof(sbyte), "integer")]
    public void ToJsonType_WithIntegerTypes_ReturnsInteger(Type type, string expected)
    {
        // Act
        var result = type.ToJsonType();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(float), "number")]
    [InlineData(typeof(double), "number")]
    [InlineData(typeof(decimal), "number")]
    public void ToJsonType_WithNumberTypes_ReturnsNumber(Type type, string expected)
    {
        // Act
        var result = type.ToJsonType();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToJsonType_WithBooleanType_ReturnsBoolean()
    {
        // Act
        var result = typeof(bool).ToJsonType();

        // Assert
        Assert.Equal("boolean", result);
    }

    [Fact]
    public void ToJsonType_WithArrayType_ReturnsArray()
    {
        // Act
        var result = typeof(Array).ToJsonType();

        // Assert
        Assert.Equal("array", result);

        var someArray = new[] { 1, 2, 3 }.GetType();

        Assert.Equal("array", someArray.ToJsonType());
    }

    [Fact]
    public void ToJsonType_WithArrayType_ReturnsArray2()
    {
        // Act
        var someArray = new[] { 1, 2, 3 }.GetType();

        // Assert
        Assert.Equal("array", someArray.ToJsonType());
    }


    [Fact]
    public void ToJsonType_WithObjectType_ReturnsObject()
    {
        // Act
        var result = typeof(object).ToJsonType();

        // Assert
        Assert.Equal("object", result);
    }

    [Theory]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(string[]))]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(IList<string>))]
    [InlineData(typeof(ICollection<object>))]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(ArrayList))]
    public void ToJsonType_WithCollectionTypes_ReturnsArray(Type type)
    {
        // Act
        var result = type.ToJsonType();

        // Assert
        Assert.Equal("array", result);
    }

    [Fact]
    public void ToJsonType_WithStringType_ReturnsString_NotArray()
    {
        // Note: string implements IEnumerable<char> but should return "string", not "array"
        // Act
        var result = typeof(string).ToJsonType();

        // Assert
        Assert.Equal("string", result);
    }

    public enum TestEnum
    {
        Value1,
        Value2
    }

    [Fact]
    public void ToJsonType_WithEnumType_ReturnsString()
    {
        // Act
        var result = typeof(TestEnum).ToJsonType();

        // Assert
        Assert.Equal("integer", result);
    }

    [Theory]
    [InlineData(typeof(ConsoleColor))]
    [InlineData(typeof(DayOfWeek))]
    [InlineData(typeof(FileAttributes))]
    public void ToJsonType_WithBuiltInEnumTypes_ReturnsString(Type enumType)
    {
        // Act
        var result = enumType.ToJsonType();

        // Assert
        Assert.Equal("integer", result);
    }

    public class CustomClass
    {
        public string Name { get; set; } = string.Empty;
    }

    public struct CustomStruct
    {
        public int Value { get; set; }
    }

    public interface ICustomInterface
    {
        void DoSomething();
    }

    [Theory]
    [InlineData(typeof(CustomClass))]
    [InlineData(typeof(CustomStruct))]
    [InlineData(typeof(ICustomInterface))]
    [InlineData(typeof(Exception))]
    [InlineData(typeof(Stream))]
    public void ToJsonType_WithCustomTypes_ReturnsObject(Type type)
    {
        // Act
        var result = type.ToJsonType();

        // Assert
        Assert.Equal("object", result);
    }

    [Theory]
    [InlineData(typeof(int?), "integer")]
    [InlineData(typeof(bool?), "boolean")]
    [InlineData(typeof(DateTime?), "string")]
    [InlineData(typeof(double?), "number")]
    [InlineData(typeof(char?), "string")]
    [InlineData(typeof(Guid?), "string")]
    [InlineData(typeof(TestEnum?), "integer")]
    public void ToJsonType_WithNullableTypes_ReturnsUnderlyingType(Type nullableType, string expectedType)
    {
        // Nullable types should return their underlying type, not "object"
        // Act
        var result = nullableType.ToJsonType();

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void ToJsonType_WithGenericType_ReturnsObject()
    {
        // Act
        var result = typeof(Dictionary<string, int>).ToJsonType();

        // Assert
        Assert.Equal("object", result);
    }

    [Fact]
    public void ToJsonType_WithNestedGenericType_ReturnsObject()
    {
        // Act
        var result = typeof(Dictionary<string, List<int>>).ToJsonType();

        // Assert
        Assert.Equal("object", result);
    }

    [Fact]
    public void ToJsonType_WithMultidimensionalArray_ReturnsArray()
    {
        // Act
        var result = typeof(int[,]).ToJsonType();

        // Assert
        Assert.Equal("array", result);
    }

    [Fact]
    public void ToJsonType_WithJaggedArray_ReturnsArray()
    {
        // Act
        var result = typeof(int[][]).ToJsonType();

        // Assert
        Assert.Equal("array", result);
    }

    [Theory]
    [InlineData(typeof(List<int?>))]
    [InlineData(typeof(IEnumerable<bool?>))]
    [InlineData(typeof(string?[]))]
    public void ToJsonType_WithNullableElementArrays_ReturnsArray(Type arrayWithNullableElements)
    {
        // Act
        var result = arrayWithNullableElements.ToJsonType();

        // Assert
        Assert.Equal("array", result);
    }

    [Theory]
    [InlineData(typeof(List<List<int>>))]
    [InlineData(typeof(IEnumerable<IEnumerable<string>>))]
    [InlineData(typeof(int[][]))]
    [InlineData(typeof(List<int[]>))]
    public void ToJsonType_WithNestedArrayTypes_ReturnsArray(Type nestedArrayType)
    {
        // Act
        var result = nestedArrayType.ToJsonType();

        // Assert
        Assert.Equal("array", result);
    }

    [Theory]
    [InlineData(typeof(Dictionary<string, List<int>>))]
    [InlineData(typeof(IDictionary<int, string>))]
    [InlineData(typeof(SortedDictionary<string, object>))]
    public void ToJsonType_WithDictionaryTypes_ReturnsObject(Type dictionaryType)
    {
        // Act
        var result = dictionaryType.ToJsonType();

        // Assert
        Assert.Equal("object", result);
    }
}

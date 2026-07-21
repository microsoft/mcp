// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class OptionTypeTests
{
    [Fact]
    public void Option_List_String_ValueType_Should_Be_Array()
    {
        // Arrange
        var option = new Option<List<string>>("--test")
        {
            Description = "Test option"
        };

        // Act
        var jsonType = option.ValueType.ToJsonType();

        // Assert
        Assert.Equal("array", jsonType);
    }

    [Fact]
    public void GetArrayElementType_Should_Return_String_For_List_String()
    {
        // Arrange
        var listType = typeof(List<string>);

        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(listType);

        // Assert
        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Array_Type()
    {
        // Arrange
        var listType = typeof(List<string>);
        var description = "A list of strings";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(listType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);
        Assert.NotNull(result.Items);
        Assert.Equal("string", result.Items.Type);
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_String_Type()
    {
        // Arrange
        var stringType = typeof(string);
        var description = "A string value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(stringType, description);

        // Assert
        Assert.Equal("string", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items);
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Integer_Type()
    {
        // Arrange
        var intType = typeof(int);
        var description = "An integer value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(intType, description);

        // Assert
        Assert.Equal("integer", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Should not have items for non-array types
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Boolean_Type()
    {
        // Arrange
        var boolType = typeof(bool);
        var description = "A boolean value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(boolType, description);

        // Assert
        Assert.Equal("boolean", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Should not have items for non-array types
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Number_Type()
    {
        // Arrange
        var doubleType = typeof(double);
        var description = "A number value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(doubleType, description);

        // Assert
        Assert.Equal("number", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Should not have items for non-array types
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Object_Type()
    {
        // Arrange
        var objectType = typeof(object);
        var description = "An object value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(objectType, description);

        // Assert
        Assert.Equal("object", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Should not have items for non-array types
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Null_Description()
    {
        // Arrange
        var stringType = typeof(string);
        string? description = null;

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(stringType, description);

        // Assert
        Assert.Equal("string", result.Type);
        Assert.Equal(string.Empty, result.Description); // Should default to empty string
        Assert.Null(result.Items);
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Integer_Array()
    {
        // Arrange
        var intArrayType = typeof(int[]);
        var description = "An array of integers";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(intArrayType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);
        Assert.NotNull(result.Items);
        Assert.Equal("integer", result.Items.Type); // Items should be integers
    }

    [Fact]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Guid_Type()
    {
        // Arrange
        var guidType = typeof(Guid);
        var description = "A GUID value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(guidType, description);

        // Assert
        Assert.Equal("string", result.Type); // GUIDs are serialized as strings
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items);
    }

    [Theory]
    [InlineData(typeof(char), "string")]
    [InlineData(typeof(DateTime), "string")]
    [InlineData(typeof(TimeSpan), "string")]
    [InlineData(typeof(uint), "integer")]
    [InlineData(typeof(long), "integer")]
    [InlineData(typeof(float), "number")]
    [InlineData(typeof(decimal), "number")]
    public void CreateOptionSchema_Should_Return_Correct_Schema_For_Various_Types(Type type, string expectedJsonType)
    {
        // Arrange
        var description = $"A {type.Name} value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(type, description);

        // Assert
        Assert.Equal(expectedJsonType, result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Non-array types should not have items
    }

    [Theory]
    [InlineData(typeof(int?), "integer")]
    [InlineData(typeof(bool?), "boolean")]
    [InlineData(typeof(DateTime?), "string")]
    [InlineData(typeof(double?), "number")]
    public void CreateOptionSchema_Should_Handle_Nullable_Types(Type nullableType, string expectedJsonType)
    {
        // Arrange
        var description = $"A nullable {nullableType.Name} value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(nullableType, description);

        // Assert
        Assert.Equal(expectedJsonType, result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Nullable types should not have items
    }

    [Fact]
    public void CreateOptionSchema_Should_Throw_ArgumentNullException_For_Null_Type()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => TypeToJsonTypeMapper.CreatePropertySchema(null!, "description"));
    }

    [Fact]
    public void GetArrayElementType_Should_Throw_ArgumentNullException_For_Null_Type()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => TypeToJsonTypeMapper.GetArrayOrCollectionElementType(null!));
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Nested_Array_Types()
    {
        // Arrange
        var nestedArrayType = typeof(List<List<string>>);
        var description = "A nested array";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(nestedArrayType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);
        Assert.NotNull(result.Items);
        Assert.Equal("array", result.Items.Type);
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Dictionary_Types()
    {
        // Arrange
        var dictType = typeof(Dictionary<string, object>);
        var description = "A dictionary";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(dictType, description);

        // Assert
        Assert.Equal("object", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items);
    }

    public enum TestEnum
    {
        Value1,
        Value2
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Enum_Types()
    {
        // Arrange
        var enumType = typeof(TestEnum);
        var description = "An enum value";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(enumType, description);

        // Assert
        Assert.Equal("integer", result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items);
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Deeply_Nested_Arrays()
    {
        // Arrange
        var deeplyNestedType = typeof(List<List<List<int>>>);
        var description = "A deeply nested array";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(deeplyNestedType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);

        // Check first level of nesting
        Assert.NotNull(result.Items);
        Assert.Equal("array", result.Items.Type);

        // Check second level of nesting
        Assert.NotNull(result.Items.Items);
        Assert.Equal("array", result.Items.Items.Type);

        // Check third level (final element type)
        Assert.NotNull(result.Items.Items.Items);
        Assert.Equal("integer", result.Items.Items.Items.Type);
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Mixed_Array_Types()
    {
        // Arrange
        var mixedArrayType = typeof(List<int[]>); // List containing arrays
        var description = "A list of integer arrays";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(mixedArrayType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);

        // Check inner array type
        Assert.NotNull(result.Items);
        Assert.Equal("array", result.Items.Type);

        // Check final element type
        Assert.NotNull(result.Items.Items);
        Assert.Equal("integer", result.Items.Items.Type);
    }

    [Theory]
    [InlineData(typeof(List<int?>), "integer")]
    [InlineData(typeof(bool?[]), "boolean")]
    [InlineData(typeof(IEnumerable<DateTime?>), "string")]
    public void CreateOptionSchema_Should_Handle_Arrays_With_Nullable_Elements(Type arrayWithNullableType, string expectedElementType)
    {
        // Arrange
        var description = "An array with nullable elements";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(arrayWithNullableType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedElementType, result.Items.Type);
    }

    [Theory]
    [InlineData(typeof(HashSet<string>), "string")]
    [InlineData(typeof(Queue<int>), "integer")]
    [InlineData(typeof(Stack<bool>), "boolean")]
    [InlineData(typeof(ISet<double>), "number")]
    public void CreateOptionSchema_Should_Handle_Other_Collection_Types(Type collectionType, string expectedElementType)
    {
        // Arrange
        var description = $"A {collectionType.Name} collection";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(collectionType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);
        Assert.NotNull(result.Items);
        Assert.Equal(expectedElementType, result.Items.Type);
    }

    [Fact]
    public void CreateOptionSchema_Should_Handle_Jagged_Arrays()
    {
        // Arrange
        var jaggedArrayType = typeof(int[][]);
        var description = "A jagged array";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(jaggedArrayType, description);

        // Assert
        Assert.Equal("array", result.Type);
        Assert.Equal(description, result.Description);

        // Check inner array type
        Assert.NotNull(result.Items);
        Assert.Equal("array", result.Items.Type);

        // Check final element type
        Assert.NotNull(result.Items.Items);
        Assert.Equal("integer", result.Items.Items.Type);
    }

    [Theory]
    [InlineData(typeof(Dictionary<string, List<int>>))]
    [InlineData(typeof(IDictionary<int, string[]>))]
    [InlineData(typeof(SortedDictionary<string, bool>))]
    public void CreateOptionSchema_Should_Handle_Complex_Dictionary_Types(Type dictionaryType)
    {
        // Arrange
        var description = "A complex dictionary";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(dictionaryType, description);

        // Assert
        Assert.Equal("object", result.Type); // Dictionaries are objects
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Objects don't have items schema
    }

    [Theory]
    [InlineData(typeof(TestEnum?), "integer")]
    [InlineData(typeof(ConsoleColor?), "integer")]
    [InlineData(typeof(DayOfWeek?), "integer")]
    public void CreateOptionSchema_Should_Handle_Nullable_Enums(Type nullableEnumType, string expectedType)
    {
        // Arrange
        var description = "A nullable enum";

        // Act
        var result = TypeToJsonTypeMapper.CreatePropertySchema(nullableEnumType, description);

        // Assert
        Assert.Equal(expectedType, result.Type);
        Assert.Equal(description, result.Description);
        Assert.Null(result.Items); // Nullable enums should not have items
    }
}

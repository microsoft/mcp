// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Azure.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class ArrayOrCollectionElementTypeTests
{
    [Theory]
    [InlineData(typeof(string[]), typeof(string))]
    [InlineData(typeof(int[]), typeof(int))]
    [InlineData(typeof(object[]), typeof(object))]
    public void GetArrayElementType_WithArrayTypes_ReturnsElementType(Type arrayType, Type expectedElementType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(arrayType);

        // Assert
        Assert.Equal(expectedElementType, result);
    }

    [Theory]
    [InlineData(typeof(List<string>), typeof(string))]
    [InlineData(typeof(List<int>), typeof(int))]
    [InlineData(typeof(IList<object>), typeof(object))]
    [InlineData(typeof(IEnumerable<bool>), typeof(bool))]
    [InlineData(typeof(ICollection<DateTime>), typeof(DateTime))]
    public void GetArrayElementType_WithGenericCollectionTypes_ReturnsElementType(Type collectionType, Type expectedElementType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(collectionType);

        // Assert
        Assert.Equal(expectedElementType, result);
    }

    [Theory]
    [InlineData(typeof(ArrayList), typeof(object))]
    [InlineData(typeof(IEnumerable), typeof(object))]
    public void GetArrayElementType_WithNonGenericCollectionTypes_ReturnsObject(Type collectionType, Type expectedElementType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(collectionType);

        // Assert
        Assert.Equal(expectedElementType, result);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(object))]
    public void GetArrayElementType_WithNonCollectionTypes_ReturnsNull(Type nonCollectionType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(nonCollectionType);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(typeof(List<int?>), typeof(int?))]
    [InlineData(typeof(IEnumerable<bool?>), typeof(bool?))]
    [InlineData(typeof(string?[]), typeof(string))] // Note: string?[] is actually string[] since string is reference type
    [InlineData(typeof(int?[]), typeof(int?))]
    public void GetArrayElementType_WithNullableElementTypes_ReturnsNullableElementType(Type collectionType, Type expectedElementType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(collectionType);

        // Assert
        Assert.Equal(expectedElementType, result);
    }

    [Theory]
    [InlineData(typeof(List<List<int>>), typeof(List<int>))]
    [InlineData(typeof(IEnumerable<IEnumerable<string>>), typeof(IEnumerable<string>))]
    [InlineData(typeof(int[][]), typeof(int[]))]
    [InlineData(typeof(List<int[]>), typeof(int[]))]
    [InlineData(typeof(int[][][]), typeof(int[][]))]
    public void GetArrayElementType_WithNestedCollections_ReturnsInnerCollectionType(Type nestedCollectionType, Type expectedInnerType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(nestedCollectionType);

        // Assert
        Assert.Equal(expectedInnerType, result);
    }

    [Theory]
    [InlineData(typeof(Dictionary<string, int>))]
    [InlineData(typeof(IDictionary<int, string>))]
    [InlineData(typeof(SortedDictionary<string, object>))]
    public void GetArrayElementType_WithDictionaryTypes_ReturnsObject(Type dictionaryType)
    {
        // Dictionary types implement IEnumerable, so they return object for the non-generic case
        // This is reasonable behavior even though they're handled as "object" in ToJsonType()
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(dictionaryType);

        // Assert
        Assert.Equal(typeof(object), result);
    }

    [Theory]
    [InlineData(typeof(HashSet<int>), typeof(int))]
    [InlineData(typeof(ISet<string>), typeof(string))]
    [InlineData(typeof(Queue<bool>), typeof(bool))]
    [InlineData(typeof(Stack<double>), typeof(double))]
    public void GetArrayElementType_WithOtherGenericCollections_ReturnsElementType(Type collectionType, Type expectedElementType)
    {
        // Act
        var result = TypeToJsonTypeMapper.GetArrayOrCollectionElementType(collectionType);

        // Assert
        Assert.Equal(expectedElementType, result);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.ObjectModel;
using Azure.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class CollectionTypeHelperTests
{
    [Theory]
    [InlineData(typeof(string[]), true)]
    [InlineData(typeof(int[]), true)]
    [InlineData(typeof(bool[]), true)]
    [InlineData(typeof(List<string>), true)]
    [InlineData(typeof(IList<string>), true)]
    [InlineData(typeof(ICollection<string>), true)]
    [InlineData(typeof(IEnumerable<string>), true)]
    [InlineData(typeof(HashSet<string>), true)]
    [InlineData(typeof(Queue<string>), true)]
    [InlineData(typeof(Stack<string>), true)]
    [InlineData(typeof(ObservableCollection<string>), true)]
    [InlineData(typeof(ArrayList), true)]
    public void IsArrayType_Should_Return_True_For_Collection_Types(Type type, bool expected)
    {
        // Act
        var result = CollectionTypeHelper.IsArrayType(type);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(Dictionary<string, object>), false)]
    [InlineData(typeof(IDictionary<string, object>), false)]
    [InlineData(typeof(SortedDictionary<string, object>), false)]
    public void IsArrayType_Should_Return_False_For_Non_Collection_Types(Type type, bool expected)
    {
        // Act
        var result = CollectionTypeHelper.IsArrayType(type);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(Dictionary<string, object>), true)]
    [InlineData(typeof(IDictionary<string, object>), true)]
    [InlineData(typeof(SortedDictionary<string, object>), true)]
    public void IsDictionaryType_Should_Return_True_For_Dictionary_Types(Type type, bool expected)
    {
        // Act
        var result = CollectionTypeHelper.IsDictionaryType(type);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string[]), false)]
    [InlineData(typeof(List<string>), false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int), false)]
    public void IsDictionaryType_Should_Return_False_For_Non_Dictionary_Types(Type type, bool expected)
    {
        // Act
        var result = CollectionTypeHelper.IsDictionaryType(type);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsArrayType_Should_Handle_Nullable_Types()
    {
        // Arrange
        var nullableIntArray = typeof(int?[]);

        // Act
        var result = CollectionTypeHelper.IsArrayType(nullableIntArray);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsArrayType_Should_Throw_For_Null_Type()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CollectionTypeHelper.IsArrayType(null!));
    }

    [Fact]
    public void IsDictionaryType_Should_Throw_For_Null_Type()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CollectionTypeHelper.IsDictionaryType(null!));
    }
}

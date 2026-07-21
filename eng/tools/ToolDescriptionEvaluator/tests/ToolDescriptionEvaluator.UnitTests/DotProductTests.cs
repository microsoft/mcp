// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using ToolSelection.VectorDb;
using Xunit;

namespace ToolDescriptionEvaluator.UnitTests;

public class DotProductTests
{
    [Fact]
    public void BiggerIsCloser_IsTrue()
    {
        Assert.True(new DotProduct().BiggerIsCloser);
    }

    [Fact]
    public void Distance_ComputesDotProduct()
    {
        var metric = new DotProduct();
        float[] a = [1f, 2f, 3f];
        float[] b = [4f, 5f, 6f];

        // 1*4 + 2*5 + 3*6 = 32
        Assert.Equal(32.0, metric.Distance(a, b), 5);
    }

    [Fact]
    public void Distance_OrthogonalVectors_ReturnsZero()
    {
        var metric = new DotProduct();
        float[] a = [1f, 0f];
        float[] b = [0f, 1f];

        Assert.Equal(0.0, metric.Distance(a, b), 5);
    }

    [Fact]
    public void Distance_MismatchedLengths_Throws()
    {
        var metric = new DotProduct();
        float[] a = [1f, 2f];
        float[] b = [1f, 2f, 3f];

        Assert.Throws<ArgumentException>(() => metric.Distance(a, b));
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using ToolSelection.VectorDb;
using Xunit;

namespace ToolDescriptionEvaluator.UnitTests;

public class CosineSimilarityTests
{
    [Fact]
    public void BiggerIsCloser_IsTrue()
    {
        Assert.True(new CosineSimilarity().BiggerIsCloser);
    }

    [Fact]
    public void Distance_IdenticalVectors_ReturnsOne()
    {
        var metric = new CosineSimilarity();
        float[] vector = [1f, 2f, 3f];

        Assert.Equal(1.0, metric.Distance(vector, vector), 5);
    }

    [Fact]
    public void Distance_OppositeVectors_ReturnsMinusOne()
    {
        var metric = new CosineSimilarity();
        float[] a = [1f, 2f, 3f];
        float[] b = [-1f, -2f, -3f];

        Assert.Equal(-1.0, metric.Distance(a, b), 5);
    }

    [Fact]
    public void Distance_OrthogonalVectors_ReturnsZero()
    {
        var metric = new CosineSimilarity();
        float[] a = [1f, 0f];
        float[] b = [0f, 1f];

        Assert.Equal(0.0, metric.Distance(a, b), 5);
    }

    [Fact]
    public void Distance_IsInvariantToMagnitude()
    {
        var metric = new CosineSimilarity();
        float[] a = [1f, 1f];
        float[] scaled = [5f, 5f];

        Assert.Equal(1.0, metric.Distance(a, scaled), 5);
    }

    [Fact]
    public void Distance_MismatchedLengths_Throws()
    {
        var metric = new CosineSimilarity();
        float[] a = [1f, 2f];
        float[] b = [1f, 2f, 3f];

        Assert.Throws<ArgumentException>(() => metric.Distance(a, b));
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolSelection.Models;

public class SuccessRateMetrics
{
    public int TopChoiceCount { get; set; }
    public int TotalTests { get; set; }

    // Updated granular confidence categories (non-mutually exclusive cumulative thresholds)
    public int VeryHighConfidenceCount { get; set; }   // ≥ 0.8
    public int HighConfidenceCount { get; set; }       // ≥ 0.7
    public int GoodConfidenceCount { get; set; }       // ≥ 0.6
    public int FairConfidenceCount { get; set; }       // ≥ 0.5
    public int AcceptableConfidenceCount { get; set; } // ≥ 0.4
    public int LowConfidenceCount { get; set; }        // < 0.4

    // Top choice + confidence combinations
    public int TopChoiceVeryHighConfidenceCount { get; set; }   // ≥ 0.8
    public int TopChoiceHighConfidenceCount { get; set; }        // ≥ 0.7
    public int TopChoiceGoodConfidenceCount { get; set; }        // ≥ 0.6
    public int TopChoiceFairConfidenceCount { get; set; }        // ≥ 0.5
    public int TopChoiceAcceptableConfidenceCount { get; set; }  // ≥ 0.4

    // Percentages
    public double TopChoicePercentage => TotalTests == 0 ? 0 : (double)TopChoiceCount / TotalTests * 100;
    public double VeryHighConfidencePercentage => TotalTests == 0 ? 0 : (double)VeryHighConfidenceCount / TotalTests * 100;
    public double HighConfidencePercentage => TotalTests == 0 ? 0 : (double)HighConfidenceCount / TotalTests * 100;
    public double GoodConfidencePercentage => TotalTests == 0 ? 0 : (double)GoodConfidenceCount / TotalTests * 100;
    public double FairConfidencePercentage => TotalTests == 0 ? 0 : (double)FairConfidenceCount / TotalTests * 100;
    public double AcceptableConfidencePercentage => TotalTests == 0 ? 0 : (double)AcceptableConfidenceCount / TotalTests * 100;
    public double LowConfidencePercentage => TotalTests == 0 ? 0 : (double)LowConfidenceCount / TotalTests * 100;

    // Top choice + confidence combination percentages
    public double TopChoiceVeryHighConfidencePercentage => TotalTests == 0 ? 0 : (double)TopChoiceVeryHighConfidenceCount / TotalTests * 100;
    public double TopChoiceHighConfidencePercentage => TotalTests == 0 ? 0 : (double)TopChoiceHighConfidenceCount / TotalTests * 100;
    public double TopChoiceGoodConfidencePercentage => TotalTests == 0 ? 0 : (double)TopChoiceGoodConfidenceCount / TotalTests * 100;
    public double TopChoiceFairConfidencePercentage => TotalTests == 0 ? 0 : (double)TopChoiceFairConfidenceCount / TotalTests * 100;
    public double TopChoiceAcceptableConfidencePercentage => TotalTests == 0 ? 0 : (double)TopChoiceAcceptableConfidenceCount / TotalTests * 100;
}

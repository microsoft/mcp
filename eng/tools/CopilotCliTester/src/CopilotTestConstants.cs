namespace CopilotCliTester;

internal static class CopilotTestConstants
{
    internal const string ModelName = "claude-opus-4.6";
    internal const int MaxRetryAttempts = 3;
    internal const int Parallel = 4;
    internal const int MaxPrompts = 0; // 0 means no limit
    internal const string OutputDirectory = "results";
    internal const double PassThreshold = 95.0;
    internal const int MaxParallelAllowed = 8;

}
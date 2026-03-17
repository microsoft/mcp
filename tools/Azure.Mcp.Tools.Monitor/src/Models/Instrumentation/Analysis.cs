namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

public record Analysis
{
    public Language Language { get; init; }
    public List<ProjectInfo> Projects { get; init; } = [];
    public InstrumentationState State { get; init; }
    public ExistingInstrumentation? ExistingInstrumentation { get; init; }
    public BrownfieldFindings? BrownfieldFindings { get; init; }
}

public record ProjectInfo
{
    public string ProjectFile { get; init; } = null!;
    public string? EntryPoint { get; init; }
    public AppType AppType { get; init; }
    public HostingPattern HostingPattern { get; init; } = HostingPattern.Unknown;
    public List<string> Dependencies { get; init; } = [];
}

public record ExistingInstrumentation
{
    public InstrumentationType Type { get; init; }
    public string? Version { get; init; }
    public List<Evidence> Evidence { get; init; } = [];
}

public record Evidence
{
    public string File { get; init; } = null!;
    public string Indicator { get; init; } = null!;
}

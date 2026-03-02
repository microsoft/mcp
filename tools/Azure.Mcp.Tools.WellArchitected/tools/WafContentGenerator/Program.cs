// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// WafContentGenerator: Fetches WAF content from MicrosoftDocs/well-architected GitHub repo
// and produces the waf-content-index.json embedded resource used by the WellArchitected toolset.
//
// Usage: dotnet run [--output <path>]
// Default output: ../../src/Resources/waf-content-index.json (relative to working directory)

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

const string BaseUrl = "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/";
const string DefaultOutputRelativePath = "../../src/Resources/waf-content-index.json";
const int MaxConcurrentRequests = 5;
const int ContentSummaryMaxLength = 500;
const int ChecklistDescriptionMaxLength = 80;

var outputPath = Path.GetFullPath(DefaultOutputRelativePath);
for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] is "--output" or "-o")
    {
        outputPath = Path.GetFullPath(args[i + 1]);
        break;
    }
}

Console.WriteLine("WAF Content Index Generator");
Console.WriteLine($"Source:  {BaseUrl}");
Console.WriteLine($"Output:  {outputPath}");
Console.WriteLine();

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WafContentGenerator/1.0");
using var semaphore = new SemaphoreSlim(MaxConcurrentRequests);

// --- Step 1: Fetch and parse TOC.yml ---
Console.WriteLine("Fetching TOC.yml...");
var tocYaml = await httpClient.GetStringAsync(BaseUrl + "TOC.yml");
var deserializer = new DeserializerBuilder().Build();
var toc = deserializer.Deserialize<List<TocEntry>>(tocYaml);

// --- Step 2: Walk TOC to extract structure ---
var pillarNames = new[] { "Reliability", "Security", "Cost Optimization", "Operational Excellence", "Performance Efficiency" };
var recommendations = new Dictionary<string, RecommendationEntry>(StringComparer.OrdinalIgnoreCase);
var serviceGuideEntries = new List<ServiceGuideEntry>();
var recIdPattern = new Regex(@"^([A-Z]{2}:\d{2})\s+(.+)$");

var pillarsSection = FindTocEntry(toc, "Pillars");
if (pillarsSection?.Items is null)
{
    Console.Error.WriteLine("ERROR: Could not find 'Pillars' section in TOC.yml");
    return 1;
}

foreach (var pillarName in pillarNames)
{
    var pillarEntry = pillarsSection.Items.FirstOrDefault(e =>
        string.Equals(e.Name, pillarName, StringComparison.OrdinalIgnoreCase));

    if (pillarEntry?.Items is null)
    {
        Console.WriteLine($"  Warning: Pillar '{pillarName}' not found in TOC");
        continue;
    }

    var strategies = pillarEntry.Items.FirstOrDefault(e =>
        e.Name?.Contains("Key design strategies", StringComparison.OrdinalIgnoreCase) == true);

    if (strategies?.Items is null) continue;

    foreach (var rec in strategies.Items)
    {
        if (rec.Name is null || rec.Href is null) continue;
        var match = recIdPattern.Match(rec.Name);
        if (!match.Success) continue;

        var id = match.Groups[1].Value;
        var title = match.Groups[2].Value.Trim();

        if (!recommendations.TryAdd(id, new RecommendationEntry(id, title, pillarName, rec.Href)))
        {
            Console.WriteLine($"  Warning: Duplicate ID {id} ('{title}'), keeping first occurrence");
        }
    }
}

// Extract service guides
var serviceGuidesSection = FindTocEntry(toc, "Service guides");
if (serviceGuidesSection?.Items is not null)
{
    foreach (var sg in serviceGuidesSection.Items)
    {
        if (sg.Name is null) continue;
        if (string.Equals(sg.Name, "Quick links", StringComparison.OrdinalIgnoreCase)) continue;

        if (sg.Href is not null)
        {
            serviceGuideEntries.Add(new ServiceGuideEntry(sg.Name, [sg.Href]));
        }
        else if (sg.Items is not null)
        {
            // Multi-page service guide (e.g., Azure SQL Managed Instance)
            var hrefs = sg.Items.Where(i => i.Href is not null).Select(i => i.Href!).ToList();
            if (hrefs.Count > 0)
            {
                serviceGuideEntries.Add(new ServiceGuideEntry(sg.Name, hrefs));
            }
        }
    }
}

// Build reverse map: normalized href path → recommendation ID
// This lets us resolve links in service guides back to rec IDs
var hrefToRecId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var rec in recommendations.Values)
{
    // "reliability/simplify.md" → "reliability/simplify"
    var normalized = rec.Href.TrimStart('.', '/').Replace(".md", "", StringComparison.OrdinalIgnoreCase);
    hrefToRecId[normalized] = rec.Id;
}

Console.WriteLine($"Found {recommendations.Count} recommendations across {pillarNames.Length} pillars");
Console.WriteLine($"Found {serviceGuideEntries.Count} service guides");
Console.WriteLine();

// --- Step 3: Fetch recommendation markdown ---
Console.WriteLine("Fetching recommendation content...");
await Parallel.ForEachAsync(recommendations.Values, new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentRequests }, async (rec, ct) =>
{
    try
    {
        var url = BaseUrl + rec.Href.TrimStart('.', '/');
        var markdown = await httpClient.GetStringAsync(url, ct);
        rec.Description = MarkdownHelper.ExtractDescription(markdown);
        rec.Content = MarkdownHelper.ExtractContentSummary(markdown, ContentSummaryMaxLength);
        Console.WriteLine($"  \u2713 {rec.Id} {rec.Title}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  \u2717 {rec.Id} {rec.Title}: {ex.Message}");
    }
});

// --- Step 4: Fetch service guide markdown ---
Console.WriteLine("\nFetching service guide content...");
var serviceGuides = new Dictionary<string, ServiceGuideOutput>();
var sgLock = new object();

await Parallel.ForEachAsync(serviceGuideEntries, new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentRequests }, async (sg, ct) =>
{
    try
    {
        var allContent = new StringBuilder();
        var allRecIds = new HashSet<string>();

        foreach (var href in sg.Hrefs)
        {
            var url = BaseUrl + href.TrimStart('.', '/');
            var markdown = await httpClient.GetStringAsync(url, ct);
            allContent.AppendLine(markdown);

            // Strategy 1: Check for explicit rec ID mentions (e.g. RE:01)
            foreach (var id in MarkdownHelper.ExtractRecommendationIds(markdown))
            {
                allRecIds.Add(id);
            }

            // Strategy 2: Resolve links to recommendation pages via href→recId map
            // Matches both relative (../reliability/simplify.md) and absolute (/azure/well-architected/reliability/simplify) links
            foreach (var id in MarkdownHelper.ResolveLinkedRecommendationIds(markdown, hrefToRecId))
            {
                allRecIds.Add(id);
            }
        }

        var key = DeriveServiceGuideKey(sg.Hrefs[0], sg.Name);
        var content = MarkdownHelper.ExtractContentSummary(allContent.ToString(), 300)
            ?? $"Well-Architected guidance for {sg.Name}.";
        var recIds = allRecIds.OrderBy(id => id).ToList();

        lock (sgLock)
        {
            serviceGuides[key] = new ServiceGuideOutput
            {
                Service = key,
                Content = content,
                RecommendationIds = recIds
            };
        }

        Console.WriteLine($"  \u2713 {sg.Name} ({recIds.Count} recommendation refs)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  \u2717 {sg.Name}: {ex.Message}");
    }
});

// --- Step 5: Assemble output ---
Console.WriteLine("\nBuilding content index...");

var recsOutput = recommendations.Values
    .OrderBy(r => r.Id)
    .ToDictionary(
        r => r.Id,
        r => new RecommendationOutput
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description ?? $"WAF recommendation {r.Id}",
            Pillar = r.Pillar,
            Content = r.Content ?? "",
            Service = null
        });

var checklists = pillarNames.ToDictionary(
    pillar => pillar.ToLowerInvariant(),
    pillar => new ChecklistOutput
    {
        Pillar = pillar,
        Items = recommendations.Values
            .Where(r => r.Pillar == pillar)
            .OrderBy(r => r.Id)
            .Select(r => new ChecklistItemOutput
            {
                Id = r.Id,
                Title = r.Title,
                Description = TruncateText(r.Description ?? "", ChecklistDescriptionMaxLength)
            })
            .ToList()
    });

var output = new ContentIndexOutput
{
    Recommendations = recsOutput,
    Checklists = checklists,
    ServiceGuides = serviceGuides
        .OrderBy(kv => kv.Key)
        .ToDictionary(kv => kv.Key, kv => kv.Value)
};

// --- Step 6: Write JSON ---
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var json = JsonSerializer.Serialize(output, jsonOptions);
await File.WriteAllTextAsync(outputPath, json);

var fileInfo = new FileInfo(outputPath);
Console.WriteLine($"\n\u2713 Written {fileInfo.Length:N0} bytes to {outputPath}");
Console.WriteLine($"  {recsOutput.Count} recommendations, {checklists.Count} checklists, {serviceGuides.Count} service guides");
return 0;

// ========== Helper Methods ==========

static TocEntry? FindTocEntry(List<TocEntry>? entries, string name)
{
    if (entries is null) return null;
    foreach (var entry in entries)
    {
        if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            return entry;
        var found = FindTocEntry(entry.Items, name);
        if (found != null) return found;
    }
    return null;
}

static string DeriveServiceGuideKey(string href, string displayName)
{
    // Extract the filename stem: "service-guides/azure-kubernetes-service.md" -> "azure-kubernetes-service"
    // For multi-page: "service-guides/azure-sql-managed-instance/reliability.md" -> "azure-sql-managed-instance"
    var parts = href.Replace('\\', '/').Split('/');
    if (parts.Length >= 2)
    {
        var parent = parts.Length >= 3 ? parts[^2] : null;
        if (parent is not null && parent != "service-guides")
        {
            return parent;
        }

        var stem = Path.GetFileNameWithoutExtension(parts[^1]);

        // Normalize known mismatches between filenames and the service guide key convention
        // Filenames in the WAF repo don't always match the azure-* pattern used by our service map
        return stem switch
        {
            "app-service-web-apps" => "azure-app-service",
            "cosmos-db" => "azure-cosmos-db",
            "virtual-machines" => "azure-virtual-machines",
            "virtual-network" => "azure-virtual-network",
            "postgresql" => "azure-database-for-postgresql",
            _ => stem
        };
    }

    return displayName.ToLowerInvariant().Replace(' ', '-');
}

static string TruncateText(string text, int maxLength)
{
    if (text.Length <= maxLength) return text;
    return text[..maxLength].TrimEnd() + "...";
}

// ========== Markdown Parsing ==========

static class MarkdownHelper
{
    private static readonly Regex s_frontMatterRegex = new(@"^---\s*\n(.*?)\n---\s*\n?", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex s_descriptionRegex = new(@"description:\s*[""']?([^""'\n]+)[""']?", RegexOptions.Compiled);
    private static readonly Regex s_recIdRegex = new(@"\b([A-Z]{2}:\d{2})\b", RegexOptions.Compiled);
    private static readonly Regex s_relativeWafLinkRegex = new(@"\]\(\.\./([a-z-]+/[a-z0-9-]+)(?:\.md)?(?:#[^)]*)?\)", RegexOptions.Compiled);
    private static readonly Regex s_absoluteWafLinkRegex = new(@"\]\(/azure/well-architected/([a-z-]+/[a-z0-9-]+)(?:#[^)]*)?\)", RegexOptions.Compiled);
    private static readonly Regex s_markdownLinkRegex = new(@"\[([^\]]+)\]\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex s_boldItalicRegex = new(@"\*{1,2}([^*]+)\*{1,2}", RegexOptions.Compiled);
    private static readonly Regex s_inlineCodeRegex = new(@"`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex s_htmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    public static string? ExtractDescription(string markdown)
    {
        // Try YAML front matter description first
        var fmMatch = s_frontMatterRegex.Match(markdown);
        if (fmMatch.Success)
        {
            var descMatch = s_descriptionRegex.Match(fmMatch.Groups[1].Value);
            if (descMatch.Success)
            {
                return CleanText(descMatch.Groups[1].Value.Trim());
            }
        }

        // Fall back to first body paragraph
        return ExtractFirstParagraph(StripFrontMatter(markdown));
    }

    public static string? ExtractContentSummary(string markdown, int maxLength)
    {
        var body = StripFrontMatter(markdown);
        var lines = body.Split('\n');
        var sb = new StringBuilder();
        var paragraphCount = 0;
        var inParagraph = false;

        foreach (var line in lines)
        {
            if (sb.Length >= maxLength) break;
            var trimmed = line.Trim();

            // Skip non-prose content
            if (IsNonProseContent(trimmed)) continue;

            if (string.IsNullOrEmpty(trimmed))
            {
                if (inParagraph)
                {
                    paragraphCount++;
                    if (paragraphCount >= 3) break;
                    inParagraph = false;
                }
                continue;
            }

            inParagraph = true;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(CleanText(trimmed));
        }

        if (sb.Length == 0) return null;
        if (sb.Length <= maxLength) return sb.ToString();

        var text = sb.ToString()[..maxLength];
        var lastSentenceEnd = text.LastIndexOf(". ", StringComparison.Ordinal);
        if (lastSentenceEnd > maxLength / 2)
            return text[..(lastSentenceEnd + 1)];
        return text.TrimEnd() + "...";
    }

    public static List<string> ExtractRecommendationIds(string markdown)
    {
        return s_recIdRegex.Matches(markdown)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
    }

    public static List<string> ResolveLinkedRecommendationIds(string markdown, Dictionary<string, string> hrefToRecId)
    {
        var ids = new HashSet<string>();

        // Match relative links: ](../reliability/simplify.md) or ](../reliability/simplify.md#section)
        foreach (Match match in s_relativeWafLinkRegex.Matches(markdown))
        {
            var path = match.Groups[1].Value;
            if (hrefToRecId.TryGetValue(path, out var id))
                ids.Add(id);
        }

        // Match absolute links: ](/azure/well-architected/reliability/simplify) or with #fragment
        foreach (Match match in s_absoluteWafLinkRegex.Matches(markdown))
        {
            var path = match.Groups[1].Value;
            if (hrefToRecId.TryGetValue(path, out var id))
                ids.Add(id);
        }

        return ids.OrderBy(id => id).ToList();
    }

    private static string StripFrontMatter(string markdown)
    {
        var match = s_frontMatterRegex.Match(markdown);
        return match.Success ? markdown[match.Length..] : markdown;
    }

    private static string? ExtractFirstParagraph(string body)
    {
        var lines = body.Split('\n');
        var sb = new StringBuilder();
        var inParagraph = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (IsNonProseContent(trimmed)) continue;

            if (string.IsNullOrEmpty(trimmed))
            {
                if (inParagraph && sb.Length > 0) break;
                continue;
            }

            inParagraph = true;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(CleanText(trimmed));
        }

        return sb.Length > 0 ? sb.ToString() : null;
    }

    private static bool IsNonProseContent(string trimmedLine) =>
        trimmedLine.StartsWith('#') ||
        trimmedLine.StartsWith(":::") ||
        trimmedLine.StartsWith("[!") ||
        trimmedLine.StartsWith("![") ||
        trimmedLine.StartsWith('|') ||
        trimmedLine.StartsWith("```") ||
        trimmedLine.StartsWith("- name:") ||
        trimmedLine.StartsWith("  href:");

    private static string CleanText(string text)
    {
        text = s_markdownLinkRegex.Replace(text, "$1");
        text = s_boldItalicRegex.Replace(text, "$1");
        text = s_inlineCodeRegex.Replace(text, "$1");
        text = s_htmlTagRegex.Replace(text, "");
        return text.Trim();
    }
}

// ========== TOC Model (YamlDotNet) ==========

class TocEntry
{
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "href")]
    public string? Href { get; set; }

    [YamlMember(Alias = "items")]
    public List<TocEntry>? Items { get; set; }
}

// ========== Internal Models ==========

class RecommendationEntry(string id, string title, string pillar, string href)
{
    public string Id { get; } = id;
    public string Title { get; } = title;
    public string Pillar { get; } = pillar;
    public string Href { get; } = href;
    public string? Description { get; set; }
    public string? Content { get; set; }
}

record ServiceGuideEntry(string Name, List<string> Hrefs);

// ========== Output Models ==========

class ContentIndexOutput
{
    public Dictionary<string, RecommendationOutput> Recommendations { get; set; } = [];
    public Dictionary<string, ChecklistOutput> Checklists { get; set; } = [];
    public Dictionary<string, ServiceGuideOutput> ServiceGuides { get; set; } = [];
}

class RecommendationOutput
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Pillar { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Service { get; set; }
}

class ChecklistOutput
{
    public string Pillar { get; set; } = "";
    public List<ChecklistItemOutput> Items { get; set; } = [];
}

class ChecklistItemOutput
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

class ServiceGuideOutput
{
    public string Service { get; set; } = "";
    public string Content { get; set; } = "";
    public List<string> RecommendationIds { get; set; } = [];
}

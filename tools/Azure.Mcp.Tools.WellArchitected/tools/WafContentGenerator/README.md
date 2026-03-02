# WAF Content Index Generator

Fetches Azure Well-Architected Framework content from the public [MicrosoftDocs/well-architected](https://github.com/MicrosoftDocs/well-architected) GitHub repository and produces the `waf-content-index.json` embedded resource used by the WellArchitected MCP toolset.

## What it does

1. Fetches `TOC.yml` from the WAF repo to discover the content structure
2. Parses all **recommendation pages** (RE:01–RE:10, SE:01–SE:12, CO:01–CO:14, OE:01–OE:11, PE:01–PE:12) — extracts titles, descriptions from YAML front matter, and content summaries from the markdown body
3. Parses all **service guide pages** (~30+ Azure services) — extracts content summaries and scans for referenced recommendation IDs
4. Builds **checklists** for each pillar from the discovered recommendations
5. Writes a single `waf-content-index.json` file matching the schema expected by `WellArchitectedService`

## Usage

Run from this directory to update the embedded resource:

```powershell
cd tools/Azure.Mcp.Tools.WellArchitected/tools/WafContentGenerator
dotnet run
```

This writes to `../../src/Resources/waf-content-index.json` by default.

To write to a custom path:

```powershell
dotnet run -- --output path/to/output.json
```

## When to run

Run this tool whenever the upstream WAF content changes and you want to refresh the embedded resource. The WellArchitected MCP toolset loads `waf-content-index.json` as a compiled embedded resource, so after regenerating, rebuild the main toolset:

```powershell
dotnet build ../../src
```

## Requirements

- .NET 10.0+
- Internet access (fetches from `raw.githubusercontent.com`)

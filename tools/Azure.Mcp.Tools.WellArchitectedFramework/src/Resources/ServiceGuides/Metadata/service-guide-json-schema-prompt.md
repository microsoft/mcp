<!-- Run below prompt in Copilot to generate the 'service-guide-json-schema.json' file. No need to include this comment in the prompt. -->

# Task: Design JSON Schema for Azure Well-Architected Framework Service Guides

Analyze all markdown files referenced by the `serviceGuideUrl` fields in `service-guides-metadata.json` and create a JSON Schema (draft-07+) that captures key information for AI agent consumption and cross-service analysis.

## Schema Structure

### Top-Level Structure

```
service-guide-schema.json
├── metadata               # Document provenance and versioning
├── serviceInfo
│   ├── serviceName
│   └── description
├── pillars                # Well-Architected Framework pillars
│   ├── reliability
│   ├── security
│   ├── costOptimization
│   ├── operationalExcellence
│   └── performanceEfficiency
├── tradeoffs[]            # Design trade-offs and implications
├── azurePolicies[]        # Azure Policy information
├── exampleArchitectures[] # Reference architectures
└── relatedResources[]     # Additional documentation
```

### Per Pillar Structure

```
pillar
├── purpose                     # What this pillar achieves
├── designRecommendations[]     # Strategic design considerations
│   ├── title
│   ├── description
├── configurationRecommendations[] # Tactical, actionable steps
│   ├── recommendation
│   ├── benefit
```

## Schema Requirements

1. **Consistent** structure across all services
2. **Extensible** for service-specific nuances
3. **Queryable** by pillar
4. **Actionable** with practical implementation guidance

## Deliverable

Produce `service-guide-json-schema.json` with property definitions, validation rules, and documentation. Sample at least 10 markdown files to identify common patterns and ensure 90%+ content coverage.
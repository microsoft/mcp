<!-- Run below prompt in Copilot to generate the service-guides.json file. No need to include this comment in the prompt. -->

Analyze the TOC.yml file from https://github.com/MicrosoftDocs/well-architected/blob/main/well-architected/TOC.yml and generate a JSON file with Azure service guide mappings.

TASK:
1. Find the "Service guides" section in the TOC.yml (look for "- name: Service guides")
2. Extract all Azure service entries under this section (exclude "Quick links")
3. Maintain the same order as they appear in TOC.yml
4. For each service, create a JSON entry with the following structure:

REQUIRED JSON STRUCTURE:
{
  "<service-key>": {
    "serviceNameVariationsNormalized": ["<variation1>", "<variation2>", ...],
    "serviceGuideUrl": "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/<href-path>"
  }
}

RULES FOR GENERATION:
1. **service-key**: Extract from the href filename (e.g., "azure-blob-storage" from "service-guides/azure-blob-storage.md")
   - Skip entries that have nested items instead of direct href (these are multi-page guides)

2. **serviceNameVariationsNormalized**: Generate intelligent variations by:
   - Removing all spaces and hyphens to create concatenated version (e.g., "azureblobstorage")
   - Creating common abbreviations and shorthand (e.g., "blob", "blobstorage", "azureblob")
   - Removing "azure-" prefix variations (e.g., "blob-storage")
   - Include common alternate names (e.g., "aks" for "azure-kubernetes-service")
   - Keep all variations lowercase and free of hyphens and spaces
   - Sort all the variations alphabetically
   - Examples:
     * "azure-blob-storage" → ["azureblob", "azureblobstorage", "blob", "blobstorage"]
     * "azure-kubernetes-service" → ["aks", "azurek8s", "azurekubernetes", "azurekubernetesservice", "k8s", "kubernetes"]
     * "application-insights" → ["applicationinsights", "appinsights", "insights"]

3. **serviceGuideUrl**: Construct as:
   - Base: "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/"
   - Append the href value from TOC.yml (e.g., "service-guides/azure-blob-storage.md")

SPECIAL HANDLING:
- If a service has nested items (like Azure SQL Managed Instance with "reliability" and "operational excellence" sub-pages), skip it or handle it separately
- Only include services with direct href links to markdown files
- Exclude the "Quick links" entry (href: service-guides/index.yml)

OUTPUT FORMAT:
- Valid JSON with proper formatting (2-space indentation)
- No trailing commas

EXAMPLE OUTPUT:
{
  "application-insights": {
    "serviceNameVariationsNormalized": ["applicationinsights", "appinsights", "insights"],
    "serviceGuideUrl": "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/application-insights.md"
  },
  "azure-blob-storage": {
    "serviceNameVariationsNormalized": ["azureblob", "azureblobstorage", "blob", "blobstorage"],
    "serviceGuideUrl": "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/azure-blob-storage.md"
  }
}

Generate the complete service-guides.json file with all services found in the Service guides section.


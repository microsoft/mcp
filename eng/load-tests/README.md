# MCP Server Load Testing

This directory contains the infrastructure for **Locust-based load testing** of the remote Azure MCP Server, integrated with **Azure Load Testing**. The load test pipeline is **on-demand only** — it never runs automatically.

## How it works

When you manually queue the pipeline (`eng/pipelines/load-test.yml`):

1. **Builds** the MCP Server as a Docker image and pushes it to Azure Container Registry (ACR)
2. **Deploys** an ephemeral Azure Container App running the server with `--dangerously-disable-http-incoming-auth` (no OAuth token required)
3. **Runs** a Locust-based load test via Azure Load Testing against the live Container App
4. **Tears down** the Container App and removes the image tag from ACR

## What is tested?

| Tool | Description |
|------|-------------|
| `azmcp_storage_account_get` | Lists / retrieves Azure Storage account details via the MCP Streamable HTTP protocol |

Each simulated Locust user follows the full MCP session lifecycle:

1. **`initialize`** – establishes a session with the server
2. **`notifications/initialized`** – signals the client is ready
3. **`tools/call`** – repeatedly invokes `azmcp_storage_account_get`

## Directory layout

```
eng/load-tests/
├── README.md                   # This file
├── locustfile.py               # Locust test script (MCP protocol)
├── config.yaml                 # Azure Load Testing configuration
└── load-test-resources.bicep   # Bicep template (ALT + Container App Environment + Container App)

eng/pipelines/
└── load-test.yml               # Azure DevOps pipeline definition (manual trigger only)
```

## Prerequisites

1. **Azure Container Registry (ACR)** – to host the per-run Docker images.

2. **Azure Load Testing resource + Container App Environment** – provisioned by the Bicep template on first run (the pipeline calls `az deployment group create`).

3. **Azure DevOps variable group** – create a variable group named `mcp-load-test-vars` with:

   | Variable | Secret? | Description |
   |----------|---------|-------------|
   | `AZURE_SERVICE_CONNECTION` | No | Azure service connection name |
   | `AZURE_LOAD_TEST_RESOURCE_GROUP` | No | Resource group for all load-test resources |
   | `AZURE_LOAD_TEST_NAME` | No | Name of the Azure Load Testing resource |
   | `AZURE_ACR_NAME` | No | Azure Container Registry name (e.g. `mcploadtestacr`) |
   | `MCP_SUBSCRIPTION` | No | Azure subscription for the storage tool call |

   > **Note:** No `MCP_ACCESS_TOKEN` is needed — the server is deployed with auth disabled.

## Provisioning infrastructure (first time)

The Bicep template provisions (idempotently):
- Azure Load Testing resource
- Log Analytics workspace
- Container App Environment
- Container App (image reference passed as parameter)

```bash
# One-time: create the resource group and ACR
az group create -n mcp-load-test-rg -l eastus2
az acr create -n mcploadtestacr -g mcp-load-test-rg --sku Basic --admin-enabled true

# The Bicep deployment happens automatically in the pipeline,
# but you can also run it manually for testing:
az deployment group create \
  --resource-group mcp-load-test-rg \
  --template-file eng/load-tests/load-test-resources.bicep \
  --parameters \
      mcpServerImage=mcploadtestacr.azurecr.io/azure-mcp-server:latest \
      containerAppName=mcp-loadtest-manual \
      acrName=mcploadtestacr
```

## Running locally (for development)

You can run the Locust test locally against a development server:

```bash
# Start the MCP server in unauthenticated HTTP mode
dotnet run --project servers/Azure.Mcp.Server/src -- server start \
  --transport http \
  --dangerously-disable-http-incoming-auth

# In another terminal, run Locust
pip install locust
cd eng/load-tests
MCP_SERVER_URL=http://localhost:5000 \
MCP_SUBSCRIPTION=<your-subscription> \
locust --headless -u 5 -r 1 -t 60s --host http://localhost:5000
```

Open http://localhost:8089 (without `--headless`) for the Locust web UI.

## Triggering a load test

The pipeline has **no automatic triggers** (no CI, no schedule, no PR trigger). To run it:

1. Go to **Azure DevOps → Pipelines → load-test**
2. Click **Run pipeline**
3. Optionally override parameters (`engineInstances`, `users`, `spawnRate`, `duration`)
4. Click **Run**

## Failure criteria

| Metric | Threshold |
|--------|-----------|
| Average response time | < 5 000 ms |
| Error rate | < 10 % |

If either threshold is exceeded, the pipeline fails. Adjust thresholds in [config.yaml](config.yaml).

## Customising test parameters

The pipeline exposes parameters that can be overridden at queue time:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `engineInstances` | 1 | Number of Azure Load Testing engine instances |
| `users` | 10 | Concurrent simulated users |
| `spawnRate` | 2 | Users spawned per second |
| `duration` | 120 | Test duration in seconds |

## Cleanup

The pipeline's **Cleanup** stage runs unconditionally (`condition: always()`) and:
- Deletes the Container App
- Removes the image tag from ACR

The Container App Environment and Azure Load Testing resource are long-lived and reused across runs.

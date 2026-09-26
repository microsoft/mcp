param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# $testSettings contains:
# - TenantId
# - TenantName
# - SubscriptionId
# - SubscriptionName
# - ResourceGroupName
# - ResourceBaseName

# $DeploymentOutputs keys are all UPPERCASE

# ---------------------------------------------------------------------------
# Seed the Items, TextItems, and VectorItems containers so the Cosmos live
# tests (CosmosCommandTests.cs) have data to read/query/search. The account
# has disableLocalAuth: true (see test-resources.bicep), so seeding uses an
# AAD bearer token against the Cosmos DB data-plane REST API rather than a
# master key.
# ---------------------------------------------------------------------------

$accountName = $testSettings.ResourceBaseName
$databaseName = "ToDoList"
$openAiEndpoint = $DeploymentOutputs["OPENAIENDPOINT"]
$embeddingDeploymentName = $DeploymentOutputs["EMBEDDINGDEPLOYMENTNAME"]
$cosmosResourceUrl = "https://$accountName.documents.azure.com/"

function Get-PlainTextAccessToken {
    # Get-AzAccessToken returns a SecureString .Token on newer Az.Accounts versions.
    param([Parameter(Mandatory)] [string] $ResourceUrl)

    $token = (Get-AzAccessToken -ResourceUrl $ResourceUrl).Token
    if ($token -is [System.Security.SecureString]) {
        return [System.Net.NetworkCredential]::new('', $token).Password
    }

    return $token
}

function Get-CosmosAuthHeader {
    param([string] $ResourceUrl)
    $token = Get-PlainTextAccessToken -ResourceUrl $ResourceUrl.TrimEnd('/')
    return [System.Uri]::EscapeDataString("type=aad&ver=1.0&sig=$token")
}

function Invoke-CosmosUpsertItem {
    param(
        [Parameter(Mandatory = $true)] [string] $ContainerName,
        [Parameter(Mandatory = $true)] [hashtable] $Document
    )

    $id = $Document["id"]
    $uri = "$cosmosResourceUrl" + "dbs/$databaseName/colls/$ContainerName/docs"
    $body = $Document | ConvertTo-Json -Depth 10 -Compress

    $headers = @{
        "Authorization"                = Get-CosmosAuthHeader -ResourceUrl $cosmosResourceUrl
        "x-ms-date"                    = [DateTime]::UtcNow.ToString("r")
        "x-ms-version"                 = "2020-07-15"
        # Items/TextItems/VectorItems all use /id as the partition key (see test-resources.bicep).
        "x-ms-documentdb-partitionkey" = "[`"$id`"]"
        "x-ms-documentdb-is-upsert"    = "true"
        "Content-Type"                 = "application/json"
    }

    Write-Host "Seeding $ContainerName/$id ..."
    Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -Body $body | Out-Null
}

function Get-Embedding {
    param([string] $Text)

    $aoaiToken = Get-PlainTextAccessToken -ResourceUrl "https://cognitiveservices.azure.com"
    $uri = "$($openAiEndpoint.TrimEnd('/'))/openai/deployments/$embeddingDeploymentName/embeddings?api-version=2024-06-01"
    $headers = @{
        "Authorization" = "Bearer $aoaiToken"
        "Content-Type"  = "application/json"
    }
    $body = @{ input = $Text } | ConvertTo-Json -Compress

    $response = Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -Body $body
    return @($response.data[0].embedding)
}

# Items container — plain to-do documents.
# Exercised by: Should_get_item_by_id, Should_get_item_by_id_with_partition_key,
# Should_list_recent_items, Should_infer_container_schema.
$todoItems = @(
    @{ id = "todo-1"; title = "Buy milk";        completed = $false; priority = 1 }
    @{ id = "todo-2"; title = "Write tests";     completed = $true;  priority = 2 }
    @{ id = "todo-3"; title = "Push recordings"; completed = $false; priority = 1 }
)
foreach ($item in $todoItems) {
    Invoke-CosmosUpsertItem -ContainerName "Items" -Document $item
}

# TextItems container — full-text search documents.
# Exercised by: Should_text_search_documents, Should_text_search_documents_with_select_properties.
# text-1 and text-3 must contain "cosmos" in their `description`; text-2 must not.
$textItems = @(
    @{ id = "text-1"; description = "hello world from cosmos full text search" }
    @{ id = "text-2"; description = "a different document about something else" }
    @{ id = "text-3"; description = "another cosmos entry to ensure multi-hit full-text results" }
)
foreach ($item in $textItems) {
    Invoke-CosmosUpsertItem -ContainerName "TextItems" -Document $item
}

# VectorItems container — 1536-dim vector-search documents (text-embedding-3-small).
# Exercised by: Should_vector_search_documents, Should_vector_search_documents_with_select_properties.
# "vec-greeting" must rank first for the query text "hello world".
$vectorTexts = @(
    @{ id = "vec-greeting";   text = "Hello world, a friendly greeting to everyone." }
    @{ id = "vec-weather";    text = "Today the weather is sunny with mild temperatures and a light breeze." }
    @{ id = "vec-recipe";     text = "A simple recipe for chocolate chip cookies with butter, sugar, and flour." }
    @{ id = "vec-code";       text = "A Python code snippet for sorting a list of dictionaries by key." }
    @{ id = "vec-astronomy";  text = "The Andromeda galaxy is the closest large spiral galaxy to the Milky Way." }
)

if ($openAiEndpoint -and $embeddingDeploymentName) {
    foreach ($item in $vectorTexts) {
        $vector = Get-Embedding -Text $item.text
        Invoke-CosmosUpsertItem -ContainerName "VectorItems" -Document @{
            id     = $item.id
            text   = $item.text
            vector = $vector
        }
    }
}
else {
    Write-Warning "Skipping VectorItems seeding: OPENAIENDPOINT / EMBEDDINGDEPLOYMENTNAME deployment outputs were not found."
}

Write-Host "Cosmos seed data complete."


---
title: Long running operations
description: This article describes the long running operations (LRO) for Microsoft Fabric REST APIs, including how to handle responses and poll for results.
author: billmath
ms.author: billmath
ms.topic: how-to
ms.service: fabric
ms.date: 03/14/2025
#customer intent: As a Microsoft Fabric user I want to learn about Long running operations for Fabric REST API's.
---

# Long running operations

A long running operation (LRO) is an asynchronous time-consuming task that allows asynchronous polling while maintaining responsiveness and scalability. Polling refers to the client pattern required to periodically check operation status until the operation completes.

In the context of web services and APIs, LROs refer to tasks or processes that require a substantial amount of time to execute, making LROs unsuitable for immediate completion within a single request-response cycle.

Examples of such operations include large data uploads, complex computations, batch processing, or resource provisioning in cloud environments.
Not all long running operations have a result in Fabric. Some operations just run to completion without providing any result URL.

## API Specification

An API that uses the LRO infrastructure returns one of two successful results:

- HTTP status code 200 OK or 201 CREATED - The response body contains the API result, if a result exists.
- HTTP status code 202 Accepted - The response body is empty.

Three response headers are automatically added by the LRO infrastructure:

- `Location header`: Contains the URL for polling the operation’s status.
- `x-ms-operation-id header`: Contains the operation ID, which can be used to construct the operation status URL.
- `Retry-After header`: Contains an integer representing the number of seconds a caller should wait before querying the operation’s status.

### Polling on state and getting result

There are two approaches you can take for polling on state and getting the result once the operation is completed:

- Using **location header**: the location header returned, while the operation is running, is the Get Operation State API with the operation ID for the ongoing operation. Once the operation completes running, the location header returned is the Get Operation Result API with the operation ID and the result.

- Using **x-ms-operation-id**: you can build API calls that return in the operation header using the x-ms-operation-id header that is returned in the response of the initial call. Pull the state with Get Operation State API, using the operation ID, and get the result with Get Operation Result API (using the operation ID).

## C# code sample for polling the operation state

```csharp
// Get operationUrl from location header or by building it with operation ID and Get State API.  
do 
{ 
  Thread.Sleep(retryAfter * 1000); // Get retryAfter value from Retry-After header. 
  response = client.GetAsync(operationUrl).Result;  
  jsonOperation = response.Content.ReadAsStringAsync().Result; 
  operation = JsonSerializer.Deserialize<FabricOperation>(jsonOperation); 
} while (operation.status != "Succeeded" && operation.status != "Failed"); 
```

## Item creation example

This is an example of a LRO operation. In this example the user created two notebooks.

### Stage 1: Create two notebooks

Create two notebooks.

### Stage 2: Poll the notebooks

Poll the notebook cretion using [Get Operation State](https://learn.microsoft.com/rest/api/fabric/core/long-running-operations/get-operation-state).

**Notebook 1** - Returns `201`. The operation is complete.

```json
{ 
  "id": "551e6a4d-d81a-4079-b08c-25cec3cebba9", 
  "type": "Notebook", 
  "displayName": "Notebook1", 
  "description": "", 
  "workspaceId": "a91e61ef-862e-4611-9d09-9c7cc07b2519" 
} 
```

**Notebook 2** - Returns `202`. The operation isn't complete. Notebook 2 isn't created.

```json
{ 
"status": "Running", 
"createdTimeUtc": "2023-11-13T22:24:40.477Z", 
"lastUpdatedTimeUtc": "2023-11-13T22:24:41.532Z", 
"percentComplete": 25 
} 
```

### Stage 3: Poll notebook 2

Wait 20 minutes then poll notebook 2 using [Get Operation State](https://learn.microsoft.com/rest/api/fabric/core/long-running-operations/get-operation-state).

GET `https://api.fabric.microsoft.com/v1/operations/b80e135a-adca-42e7-aaf0-59849af2ed78`

**Notebook 2** - Returns `201`. The operation is complete. Notebook 2 is created.

Operation is completed - got 200-OK http status code. Response body:

```json
{ 
  "status": "Succeeded", 
  "createdTimeUtc": "2023-11-13T22:25:06.1193103", 
  "lastUpdatedTimeUtc": "2023-11-13T22:25:09.0255787", 
  "percentComplete": 100, 
  "error": null 
} 
```

### Stage 4: Get notebook 2 operation result

Get the operation result for notebook 2 with [Get Operation Result](https://learn.microsoft.com/rest/api/fabric/core/long-running-operations/get-operation-result).

GET `https://api.fabric.microsoft.com/v1/operations/b80e135a-adca-42e7-aaf0-59849af2ed78/result`

**Notebook 2** - Returns `200`. Notebook 2 is created.

```json
{ 
"id": "221a6eea-0f27-41eb-bcc5-e4d7b216ed43", 
"type": "Notebook", 
"displayName": " Notebook2", 
"description": "", 
"workspaceId": "a91e61ef-862e-4611-9d09-9c7cc07b2519" 
}  
```

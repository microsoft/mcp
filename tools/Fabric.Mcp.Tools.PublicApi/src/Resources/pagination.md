---
title: Use Pagination with Fabric REST APIs
description: Understand what pagination is, why it is important, and how to effectively use it with the Microsoft Fabric REST APIs to manage large datasets.
author: billmath
ms.author: billmath
ms.service: fabric
ms.topic: concept-article
ms.date: 03/17/2025
#customer intent: As a Microsoft Fabric user I want to learn how to use pagination with Fabric Rest APIs.
---

# Pagination

Pagination refers to the practice of breaking up a large set of data into smaller, manageable chunks or pages when delivering the data to a client application. It's a common technique used to improve the performance and efficiency of API requests, especially when dealing with a large amount of data. Pagination is also used to prevent data loss when there's too much data to display in one chunk.

## How do I know if an API is paginated?

Microsoft Fabric paginated APIs contain these parameters.

* `continuationUri`

* `continuationToken`

## Where can I find the paginated parameters?

The structure of a paginated API response contains the `continuationUri` and `continuationToken` parameters and looks like this:

```json
{
  "value": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",  
      "displayName": "Lakehouse",
      "description": "A lakehouse used by the analytics team.",
      "type": "Lakehouse",
      "workspaceId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy" 
    },
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",  
      "displayName": "Notebook",
      "description": "A notebook for refining medical data analysis through machine learning algorithms.",
      "type": "Notebook",
      "workspaceId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy" 
    }
  ],
  "continuationToken": "ABCsMTAwMDAwLDA%3D",
  "continuationUri": "https://api.fabric.microsoft.com/v1/workspaces/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/items?continuationToken=ABCsMTAwMDAwLDA%3D"
}
```

## How is pagination used in the Fabric REST APIs?

When you make a request to a paginated API, you receive a set of records, usually under the *value* property. The records include the `continuationUri` and `continuationToken` parameters. With these parameters you can retrieve the next set of records using one of these methods:

* Use `continuationUri` to get your next request.

* Use `continuationToken` as a query parameter to build your next request.

Once all the records are retrieved, the `continuationUri` and `continuationToken` parameters are removed from the response or appear as null.

## Code example

In this example, you create a client and call the [list workspaces](https://learn.microsoft.com/rest/api/fabric/admin/workspaces/list-workspaces) API. The `continuationToken` parameter is used to get the next paginated chunk of workspaces, until it returns empty or null.
In this example, you create a client and call the [list workspaces](https://learn.microsoft.com/rest/api/fabric/admin/workspaces/list-workspaces) API. The `continuationToken` parameter is used to get the next paginated chunk of workspaces, until it returns empty or null.

```csharp
using (HttpClient client = new HttpClient()) 
{ 
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<Your token>"); 
    string continuationToken = null; 
    var workspaces = new List<Workspace>(); 
    do 
    { 
        var requestUrl = "https://api.fabric.microsoft.com/v1/workspaces"; 
        if (!string.IsNullOrEmpty(continuationToken)) 
        { 
            requestUrl += $"?continuationToken={continuationToken}"; 
        } 
        HttpResponseMessage response = await client.GetAsync(requestUrl); 
        if (response.IsSuccessStatusCode) 
        { 

            // Parse the response JSON   
            var responseData = await response.Content.ReadAsStringAsync(); 
            var paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<Workspace>>(responseData); 

            // Append the list of workspaces in the current retrieved page 
            workspaces.AddRange(paginatedResponse.Value); 

            // Check if there are more records to retrieve 
            continuationToken = paginatedResponse.ContinuationToken; 
        } 
        else 
        { 
            Console.WriteLine($"Error: {response.StatusCode}"); 
            break; 
        } 
    } while (!string.IsNullOrEmpty(continuationToken)); 
}
```

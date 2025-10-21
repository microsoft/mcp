---
title: KQL Dashboard item definition
description: Learn how to structure a Real-Time Dashboard (KQL Dashboard) definition when using the Microsoft Fabric REST API.
author: ronitamir185
ms.title: Microsoft Fabric KQL Dashboard item definition
ms.author: rotamir
ms.service: fabric
ms.date: 09/19/2024
---

# KQL Dashboard definition

This article provides a breakdown of the structure for Real-Time Dashboard (KQL Dashboard) definition items.

## Supported formats

KQLDashboardDefinition items support the `JSON` format.

## Definition parts

The definition of a Real-Time Dashboard item is constructed as follows:

* **Path**: The path to the file that contains the JSON definition.
* **Payload**: See [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64)
* **PayloadType**: InlineBase64

## Example of payload content decoded from Base64

To see how to create a JSON file describing a dashboard, see [export dashboards](https://learn.microsoft.com/fabric/real-time-intelligence/dashboard-real-time-create#export-dashboards).

```json
{
 "autoRefresh": {
   "enabled": false
 },
 "baseQueries": [],
 "tiles": [],
 "dataSources": [],
 "pages": [
   {
     "name": "Page 1",
     "id": "ab1c2345-6789-01d3-4ab5-678901c2345"
   }
 ],
 "parameters": [
   {
     "kind": "duration",
     "id": "ab0c1234-6678-90d1-2ab3-456789c0123",
     "displayName": "Time range",
     "description": "",
     "beginVariableName": "_startTime",
     "endVariableName": "_endTime",
     "defaultValue": {
       "kind": "dynamic",
       "count": 1,
       "unit": "hours"
     },
     "showOnPages": {
       "kind": "all"
     }
   }
 ],
 "queries": [],
 "schema_version": "52",
 "title": "vcdzngrzsm"
}

```

## Definition example

```JSON

{
    "parts": [
      {
        "path": "RealTimeDashboard.json",
        "payload": "ewogICJhdXRvUmVmcmVzaCI6IHsKICAgICJlbmFibGVkIjogZmFsc2UKICB9LAogICJiYXNlUXVlcmllcyI6IFtdLAogICJ0aWxlcyI6IFtdLAogICJkYXRhU291cmNlcyI6IFtdLAogICJwYWdlcyI6IFsKICAgIHsKICAgICAgIm5hbWUiOiAiUGFnZSAxIiwKICAgICAgImlkIjogImFiNWYwNjI4LTk3NTAtNDNlNi05YmE0LTIwMTE2ZTg2ODc1NSIKICAgIH0KICBdLAogICJwYXJhbWV0ZXJzIjogWwogICAgewogICAgICAia2luZCI6ICJkdXJhdGlvbiIsCiAgICAgICJpZCI6ICJmNTEwYjlmZi0xZjI3LTQ4YmQtODBlNC1mZDFhNTllODQ5MjQiLAogICAgICAiZGlzcGxheU5hbWUiOiAiVGltZSByYW5nZSIsCiAgICAgICJkZXNjcmlwdGlvbiI6ICIiLAogICAgICAiYmVnaW5WYXJpYWJsZU5hbWUiOiAiX3N0YXJ0VGltZSIsCiAgICAgICJlbmRWYXJpYWJsZU5hbWUiOiAiX2VuZFRpbWUiLAogICAgICAiZGVmYXVsdFZhbHVlIjogewogICAgICAgICJraW5kIjogImR5bmFtaWMiLAogICAgICAgICJjb3VudCI6IDEsCiAgICAgICAgInVuaXQiOiAiaG91cnMiCiAgICAgIH0sCiAgICAgICJzaG93T25QYWdlcyI6IHsKICAgICAgICAia2luZCI6ICJhbGwiCiAgICAgIH0KICAgIH0KICBdLAogICJxdWVyaWVzIjogW10sCiAgInNjaGVtYV92ZXJzaW9uIjogIjUyIiwKICAidGl0bGUiOiAidmNkem5ncnpzbSIKfQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
        "payloadType": "InlineBase64"
      }
    ]
  }

```

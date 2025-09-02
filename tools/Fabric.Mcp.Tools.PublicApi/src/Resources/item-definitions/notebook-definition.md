---
title: Notebook definition
description: Learn how to structure a notebook item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric notebook item definition
author: billmath
ms.author: billmath
ms.service: fabric
ms.date: 04/23/2025
---

# Notebook definition

This article provides a breakdown of the definition structure for notebook items.

## Supported formats

Notebook items support `FabricGitSource` and `ipynb` formats. If no format is specified it will default to `FabricGitSource`.  

> [!NOTE]
>
> The `FabricGitSource` format returns notebook content in the file format corresponding to the selected language. For example, if PySpark is selected, the content will be returned as a `.py` file.

## Definition parts

The definition of a notebook item is made out of a single part, and is constructed as follows:

* **Path** : The file name, for example `artifact.content.ipynb`. 
    - for PySpark or Python: `notebook-content.py`
    - for Spark SQL: `notebook-content.sql` 
    - for Spark (Scala): `notebook-content.scala`
    - for SparkR (R): `notebook-content.r`

* **Payload type** - InlineBase64

* **Payload** See: [Example of payload content decoded from Base64](#example-of-ipynb-format-payload-content-decoded-from-base64).

## Platform part

The platform part is a file that contains the notebook metadata information.

* [Create Item](/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided. (Platform not mandatory).

* [Get Item](/rest/api/fabric/core/items/get-item) definition always returns the platform file.

* [Update Item](/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, but only if you set a new URL parameter `updateMetadata=true`.

### Example of ipynb format payload content decoded from Base64

```json
{
    "nbformat": 4,
    "nbformat_minor": 5,
    "cells": [
        {
            "cell_type": "code",
            "source": ["# Welcome to your new notebook\n# Type here in the cell editor to add code!\n"],
            "execution_count": null,
            "outputs": [],
            "metadata": {}
        }
    ],
    "metadata": {
        "language_info": {
            "name": "python"
        }
    }
}
```

## Definition example for ipynb

```json
{
    "format": "ipynb",
    "parts": [
        {
            "path": "artifact.content.ipynb",
            "payload": "eyJuYmZvcm1hdCI6NCwibmJmb3JtYXR_fbWlub3IiOjUsImNlbGxzIjpbeyJjZWxsX3R5cGUiOiJjb2RlIiwic291cmNlIjpbIiMgV2VsY29tZSB0byB5b3VyIG5ldyBub3RlYm9va1xuIyBUeXBlIGhlcmUgaW4gdGhlIGNlbGwgZWRpdG9yIHRvIGFkZCBjb2RlIVxuIl0sImV4ZWN1dGlvbl9jb3VudCI6bnVsbCwib3V0cHV0cyI6W10sIm1ldGFkYXRhIjp7fX1dLCJtZXRhZGF0YSI6eyJsYW5ndWFnZV9pbmZvIjp7Im5hbWUiOiJweXRob24ifX19",
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

### Example of fabric git resource format payload content decoded from Base64 

```python
# Fabric notebook source 
# METADATA ******************** 
# META { 
# META   "kernel_info": { 
# META     "name": "synapse_pyspark" 
# META   }, 
# META   "dependencies": {} 
# META } 
# CELL ******************** 
# Welcome to your new notebook 
# Type here in the cell editor to add code! 
# METADATA ******************** 
# META { 
# META   "language": "python", 
# META   "language_group": "synapse_pyspark" 
# META } 
```

### Definition example for fabricGitSource

```json
{
    "format": "fabricGitSource",
    "parts": [
        {
            "path": "notebook-content.py",
            "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9naXRJbnRlZ3JhdGlvbi9wbGF0Zm9ybVByb3BlcnRpZXMvMi4wLjAvc2NoZW1hLmpzb24iLAogICJtZXRhZGF0YSI6IHsKICAgICJ0eXBlIjogIk5vdGVib29rIiwKICAgICJkaXNwbGF5TmFtZSI6ICJOb3RlYm9vayA4IiwKICAgICJkZXNjcmlwdGlvbiI6ICJOZXcgbm90ZWJvb2siCiAgfSwKICAiY29uZmlnIjogewogICAgInZlcnNpb24iOiAiMi4wIiwKICAgICJsb2dpY2FsSWQiOiAiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIgogIH0KfQ==",
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
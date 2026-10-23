# Org app audience definition

This article provides a breakdown of the structure for Org app audience definition items.

## Supported formats

Org app audience items support the JSON format.

## Definition parts

This table lists the Org app audience definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `definition.json` | ContentDetails (JSON) | true | Describes the content of the payload |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

## Definition example

```json
{
  "parts": [
    {
      "path": "definition.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": ".platform",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## ContentDetails

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Defines the schema to use for an Org app audience. |
| `settings` | [AudienceSettings](#audiencesettings) | false | Defines the [settings](#audiencesettings) for the audience. |
| `parentAppId` | Guid | true | The parent organizational app ID. |
| `elementReferences` | ([ContentItemElementReference](#contentitemelementreference) \| [AppItemElementReference](#appitemelementreference))[] | true | Defines the list of [element references](#contentitemelementreference) for the audience. Maximum 1000 items. Each array item must match either [ContentItemElementReference](#contentitemelementreference) or [AppItemElementReference](#appitemelementreference). |

### AudienceSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `hasAccessToHiddenContent` | boolean | false | Indicates whether the audience has access to hidden content. <br>True - hidden content access will be shared to users in this audience. False (default) - hidden content access will not be shared. |
| `tabOrder` | integer | false | The order in which audiences will appear in the app. |

### ContentItemElementReference

Defines items that are workspace content, e.g. reports, notebooks, etc.

> [!NOTE] 
> In addition to `elementId` and `itemType` always being required, either (`itemId` + `folderObjectId`) **or** (`itemLogicalId`) must also be provided (mutually exclusive).

| Property | Type | Required | Description |
|---|---|---|---|
| `elementId` | Guid | true | Defines the item element's unique identifier. |
| `isElementHidden` | boolean | false | Indicates whether the element is hidden in the audience. <br>True - the element will not be visible in the navigation pane for this audience. False (default) - the element will be visible in the navigation pane for this audience. |
| `itemType` | string | true | The type of artifact the element references. Min length: 1, max length: 256. |
| `itemId` | Guid | conditional | Defines the item's internal unique identifier. Required when `itemLogicalId` is not provided. |
| `folderObjectId` | Guid | conditional | Defines the internal unique identifier of the folder in which the item is located. Required when `itemLogicalId` is not provided. |
| `itemLogicalId` | Guid | conditional | Defines the item's logical unique identifier in a Git repository. Required when `itemId` and `folderObjectId` are not provided. |

### AppItemElementReference

Defines items that are only in the app, e.g. links, overview, etc.

| Property | Type | Required | Description |
|---|---|---|---|
| `elementId` | Guid | true | Defines the item element's unique identifier. |
| `isElementHidden` | boolean | false | Indicates whether the element is hidden in the audience. <br>True - the element will not be visible in the navigation pane for this audience. False (default) - the element will be visible in the navigation pane for this audience. |

### File example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/orgappaudience/definition/orgAppAudienceDefinition/1.0.0/schema.json",
  "parentAppId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "settings": {
    "hasAccessToHiddenContent": false,
    "tabOrder": 1
  },
  "elementReferences": [
    {
      "elementId": "11111111-1111-1111-1111-111111111111",
      "itemType": "Report",
      "isElementHidden": false,
      "itemId": "22222222-2222-2222-2222-222222222222",
      "folderObjectId": "33333333-3333-3333-3333-333333333333"
    },
    {
      "elementId": "44444444-4444-4444-4444-444444444444",
      "itemType": "Notebook",
      "isElementHidden": true,
      "itemLogicalId": "55555555-5555-5555-5555-555555555555"
    },
    {
      "elementId": "66666666-6666-6666-6666-666666666666",
      "isElementHidden": false
    }
  ]
}
```
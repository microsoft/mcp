# Org app definition

This article provides a breakdown of the structure of Org app item definitions.

## Supported formats

Org app items support the JSON format.

## Definition parts

This table lists the Org app definition parts.

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
| `$schema` | string | true | Defines the schema to use for an Org app item. |
| `settings` | [Settings](#settings) | true | Defines the [settings](#settings) for the Org app. |
| `elements` | [Element](#element)[] | true | Defines the list of [content elements](#element). |

### Settings

| Property | Type | Required | Description |
|---|---|---|---|
| `logo` | string | false | Defines the image logo as a data URL that contains a base64-encoded image string, for example, `data:image/png;base64,<value>`. |
| `theme` | [Theme](#theme) | false | Defines the [theme](#theme) for the Org app. |
| `experienceSettings` | [ExperienceSettings](#experiencesettings) | false | Defines the [experience settings](#experiencesettings) for the Org app. |
| `itemTypeSettings` | [ItemTypeSettings](#itemtypesettings) | false | Defines the default [item-type-level settings](#itemtypesettings) for elements in the Org app. |
| `audienceSettings` | [AudienceSettings](#audiencesettings) | false | Defines the [settings for Org app audiences](#audiencesettings). |

### Theme

| Property | Type | Required | Description |
|---|---|---|---|
| `background` | string | true | Defines the main theme color for the Org app in hexadecimal format. |
| `foreground` | string | true | Defines the font color for the Org app in hexadecimal format. |
| `backgroundHover` | string | true | Defines the background color for the hover state in hexadecimal format. |
| `backgroundSelected` | string | true | Defines the background color for the selected state in hexadecimal format. |
| `backgroundPressed` | string | true | Defines the background color for the pressed state in hexadecimal format. |

### ExperienceSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `navigationPane` | [NavigationPane](#navigationpane) | false | Defines the [settings for the navigation pane](#navigationpane). |

### NavigationPane

| Property | Type | Required | Description |
|---|---|---|---|
| `isHidden` | boolean | false | Indicates whether the Org app navigation pane is visible to users. <br>True - navigation pane is hidden. False (default) - navigation pane is shown. |
| `isCollapsed` | boolean | false | Indicates whether the Org app navigation pane is expanded or collapsed for users. <br>True - navigation pane is collapsed. False (default) - navigation pane is expanded. |
| `independentPageNavigation` | boolean | false | Indicates whether to list report pages independently or as part of the navigation pane. <br>True - report pages appear separately from the navigation pane. False (default) - report pages are part of the navigation pane. |

### ItemTypeSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `report` | [ReportSettings](#reportsettings) | false | Defines the [settings for report elements](#reportsettings) in the Org app. |

### ReportSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `hidePagePane` | boolean | false | Indicates whether the report pages pane should be hidden or not. <br>True - navigation pane does not show report pages. False (default) - navigation pane includes report pages. |

### Element

Can be one of the following types:
- [OverviewElement](#overviewelement)
- [LinkElement](#linkelement)
- [SectionElement](#sectionelement)
- [ItemElement](#itemelement)

### OverviewElement

| Property | Type | Required | Description |
|---|---|---|---|
| `elementType` | [ElementType](#elementtype-enum) | true | Defines the [type](#elementtype-enum) of the overview element. Must be set to `overview`. |
| `header` | [OverviewHeader](#overviewheader) | false | Defines the [header content](#overviewheader) of the overview element. |
| `isHidden` | boolean | false | Indicates whether the overview element is visible to users. <br>True - overview element is hidden. False (default) - overview element is shown. |
| `elementId` | Guid | true | Defines the overview element's unique identifier. |
| `displayName` | string | true | Defines the display name of the overview element. |

### OverviewHeader

| Property | Type | Required | Description |
|---|---|---|---|
| `title` | string | true | Defines the title of the overview element's header. |
| `body` | string | true | Defines the body of the overview element's header. |
| `showTheme` | boolean | true | Indicates whether a theme is shown for the overview header. <br>True - if the app's theme is defined, it is also applied to the overview header. False (default) - the app's theme is not applied to the overview header. |

### LinkElement

| Property | Type | Required | Description |
|---|---|---|---|
| `elementType` | [ElementType](#elementtype-enum) | true | Defines the [type](#elementtype-enum) of the link element. Must be set to `link`. |
| `url` | string | true | Defines the URL that should be opened by the Org app. |
| `linkType` | [LinkType](#linktype-enum) | true | Defines the [link type](#linktype-enum) that determines how the URL should be opened. |
| `isHidden` | boolean | false | Indicates whether the link element is visible to users. <br>True - link element is hidden. False (default) - link element is shown. |
| `elementId` | Guid | true | Defines the link element's unique identifier. |
| `displayName` | string | true | Defines the display name of the link element. |

### SectionElement

| Property | Type | Required | Description |
|---|---|---|---|
| `elementType` | [ElementType](#elementtype-enum) | true | Defines the [type](#elementtype-enum) of the section element. Must be set to `section`. |
| `elements` | [Element](#element)[] | true | Defines the list of [elements](#element) within the section. |
| `elementId` | Guid | true | Defines the section element's unique identifier. |
| `displayName` | string | true | Defines the display name of the section element. |

### ItemElement

| Property | Type | Required | Description |
|---|---|---|---|
| `elementType` | [ElementType](#elementtype-enum) | true | Defines the [type](#elementtype-enum) of the item element. Must be set to `item`. |
| `elementId` | Guid | true | Defines the item element's unique identifier. |
| `itemId` | Guid | false | Defines the item's internal unique identifier. Required when `itemLogicalId` is not provided. |
| `folderObjectId` | Guid | false | Defines the internal unique identifier of the folder in which the item is located. Required when `itemLogicalId` is not provided. |
| `itemLogicalId` | Guid | false | Defines the item's logical unique identifier in a Git repository. Required when `itemId` and `folderObjectId` are not provided. |
| `itemType` | string | true | Defines the item's type. |
| `isHidden` | boolean | false | Indicates whether the item element is visible to users. <br>True - item element is hidden. False (default) - item element is shown. |
| `displayName` | string | true | Defines the display name of the item element. |

### AudienceSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `hideAudienceTabs` | boolean | false | Indicates whether the Org app should show audience tabs. <br>True - audience tabs will not be shown to users. False (default) - audience tabs will be shown to users. |
| `hideAllTab` | boolean | false | Indicates whether the Org app should show the All tab, which displays all content from audiences that a user has access to. <br>True - the All audience tab will not be shown to users. False (default) - the All tab will be shown to users. |

### ElementType (Enum)

| Name     | Description       |
|----------|-------------------|
| overview | Overview element. |
| link     | Link element.     |
| section  | Section element.  |
| item     | Item element.     | 

### LinkType (Enum)

| Name     | Description                         |
|----------|-------------------------------------|
| embedded | Content opens in the app.           |
| newtab   | Content opens in a new browser tab. |

### File example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/orgapp/definition/orgAppDefinition/2.0.0/schema.json",
  "settings": {
    "logo": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA",
    "theme": {
      "background": "#112233",
      "foreground": "#FFFFFF",
      "backgroundHover": "#223344",
      "backgroundSelected": "#334455",
      "backgroundPressed": "#445566"
    },
    "experienceSettings": {
      "navigationPane": {
        "isHidden": false,
        "isCollapsed": false,
        "independentPageNavigation": true
      }
    },
    "itemTypeSettings": {
      "report": {
        "hidePagePane": false
      }
    },
    "audienceSettings": {
      "hideAudienceTabs": false,
      "hideAllTab": false
    }
  },
  "elements": [
    {
      "elementType": "overview",
      "elementId": "11111111-1111-1111-1111-111111111111",
      "displayName": "Overview",
      "header": {
        "title": "Welcome to the Org App",
        "body": "This app contains curated content for your organization.",
        "showTheme": true
      },
      "isHidden": false
    },
    {
      "elementType": "link",
      "url": "https://contoso.example/path",
      "linkType": "embedded",
      "elementId": "22222222-2222-2222-2222-222222222222",
      "displayName": "Company portal",
      "isHidden": false
    },
    {
      "elementType": "section",
      "elementId": "33333333-3333-3333-3333-333333333333",
      "displayName": "Reports",
      "elements": [
        {
          "elementType": "item",
          "elementId": "44444444-4444-4444-4444-444444444444",
          "itemId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "folderObjectId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
          "itemType": "Report",
          "displayName": "Sales summary",
          "isHidden": false
        }
      ]
    }
  ]
}
```
# Ontology definition

This article provides a breakdown of the structure for ontology definition items.

## Supported formats

Ontology items support the JSON format.

## Definition parts

This table lists the ontology definition parts.

Several parts include an entity type ID value. The entity type ID is a positive 64-bit integer that's unique across the ontology instance.

| Definition part path | Type | Required | Description |
|----|----|----|----|
| `definition.json` | DefinitionDetails (JSON) | true | Empty definition. |
| `.platform` | PlatformDetails (JSON) | true | Describes common details of the item. |
| `EntityTypes/{ID}` | Directory | false | Contains the definition.json file for the entity type which describes details of the entity type. The entity type ID is a positive 64-bit integer that's unique across the ontology instance. |
| `EntityTypes/{ID}/DataBindings` | Directory | false | Contains a list of data binding files that are part of the entity type. Each data binding operation file (JSON) describes details of the data binding. |
| `EntityTypes/{ID}/Documents` | Directory | false | Contains a list of document files that are part of the entity type. Each document operation file (JSON) describes details of the document. |
| `EntityTypes/{ID}/Overviews` | Directory | false | Contains the definition.json file for the overviews of the entity type. |
| `RelationshipTypes/{ID}` | Directory | false | Contains the definition.json file for the relationship type which describes details of the relationship type. |
| `RelationshipTypes/{ID}/Contextualizations` | Directory | false | Contains a list of contextualization files that are part of the relationship type. Each contextualization file (JSON) describes details of the contextualization. |

## Definition example

```json
{
  "parts": [
    {
      "path": ".platform",
      "payload": "eyANCiAgIm1ldGFkYXRhIjogeyANCiAgICAidHlwZSI6ICJPbnRvbG9neSIsIA0KICAgICJkaXNwbGF5TmFtZSI6ICJvbnRvbG9neSIgDQogIH0gDQp9",
      "payloadType": "InlineBase64"
    },
    { 
      "path": "definition.json",
      "payload": "e30=",
      "payloadType": "InlineBase64"
    }
  ]
}
```

### Definition example with optional definition parts

```json
{
  "parts": [
    {
      "path": ".platform",
      "payload": "eyANCiAgIm1ldGFkYXRhIjogeyANCiAgICAidHlwZSI6ICJPbnRvbG9neSIsIA0KICAgICJkaXNwbGF5TmFtZSI6ICJvbnRvbG9neSIgDQogIH0gDQp9",
      "payloadType": "InlineBase64"
    },
    { 
      "path": "definition.json",
      "payload": "e30=",
      "payloadType": "InlineBase64"
    }, 
    {
      "path": "EntityTypes/8813598896083/definition.json",
      "payload": " ew0KICAiaWQiOiAiODgxMzU5ODg5NjA4MyIsDQogICJuYW1lc3BhY2UiOiAidXNlcnR5cGVzIiwNCiAgImJhc2VFbnRpdHlUeXBlSWQiOiBudWxsLA0KICAibmFtZSI6ICJFcXVpcG1lbnQxIiwNCiAgImVudGl0eUlkUGFydHMiOiBbDQogICAgIjMxMTcwNjgwMzYzNzQ1OTQwMTMiDQogIF0sDQogICJkaXNwbGF5TmFtZVByb3BlcnR5SWQiOiAiMzExNzA2ODAzNjM3NDU5NDAxMyIsDQogICJuYW1lc3BhY2VUeXBlIjogIkN1c3RvbSIsDQogICJ2aXNpYmlsaXR5IjogIlZpc2libGUiLA0KICAicHJvcGVydGllcyI6IFsNCiAgICB7DQogICAgICAiaWQiOiAiMzExNzA2ODAzNjM3NDU5NDAxMyIsDQogICAgICAibmFtZSI6ICJEaXNwbGF5TmFtZSIsDQogICAgICAicmVkZWZpbmVzIjogbnVsbCwNCiAgICAgICJiYXNlVHlwZU5hbWVzcGFjZVR5cGUiOiBudWxsLA0KICAgICAgInZhbHVlVHlwZSI6ICJTdHJpbmciDQogICAgfSwNCiAgICB7DQogICAgICAiaWQiOiAiMzExNzA2ODAzMTk1MDAwMDMzMSIsDQogICAgICAibmFtZSI6ICJNYW51ZmFjdHVyZXIiLA0KICAgICAgInJlZGVmaW5lcyI6IG51bGwsDQogICAgICAiYmFzZVR5cGVOYW1lc3BhY2VUeXBlIjogbnVsbCwNCiAgICAgICJ2YWx1ZVR5cGUiOiAiU3RyaW5nIg0KICAgIH0NCiAgXSwNCiAgInRpbWVzZXJpZXNQcm9wZXJ0aWVzIjogWw0KICAgIHsNCiAgICAgICJpZCI6ICIzMTE0NTg0OTgxMzY4Nzk2OTUzIiwNCiAgICAgICJuYW1lIjogIlByZWNpc2VUaW1lc3RhbXAiLA0KICAgICAgInJlZGVmaW5lcyI6IG51bGwsDQogICAgICAiYmFzZVR5cGVOYW1lc3BhY2VUeXBlIjogbnVsbCwNCiAgICAgICJ2YWx1ZVR5cGUiOiAiRGF0ZVRpbWUiDQogICAgfSwNCiAgICB7DQogICAgICAiaWQiOiAiMzExNDU4NDk3OTc0MzMyMDkzNCIsDQogICAgICAibmFtZSI6ICJOYW1lIiwNCiAgICAgICJyZWRlZmluZXMiOiBudWxsLA0KICAgICAgImJhc2VUeXBlTmFtZXNwYWNlVHlwZSI6IG51bGwsDQogICAgICAidmFsdWVUeXBlIjogIlN0cmluZyINCiAgICB9LA0KICAgIHsNCiAgICAgICJpZCI6ICIzMTE0NTg0OTc3NTYyNjc5NjcyIiwNCiAgICAgICJuYW1lIjogIlRlbXBlcmF0dXJlIiwNCiAgICAgICJyZWRlZmluZXMiOiBudWxsLA0KICAgICAgImJhc2VUeXBlTmFtZXNwYWNlVHlwZSI6IG51bGwsDQogICAgICAidmFsdWVUeXBlIjogIkRvdWJsZSINCiAgICB9DQogIF0NCn0= ",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/8813598896083/DataBindings/66253a71-c26f-4c9d-877f-3af5632a4be2.json",
      "payload": "ew0KICAiaWQiOiAiNjYyNTNhNzEtYzI2Zi00YzlkLTg3N2YtM2FmNTYzMmE0YmUyIiwNCiAgImRhdGFCaW5kaW5nQ29uZmlndXJhdGlvbiI6IHsNCiAgICAiZGF0YUJpbmRpbmdUeXBlIjogIk5vblRpbWVTZXJpZXMiLA0KICAgICJwcm9wZXJ0eUJpbmRpbmdzIjogWw0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJEaXNwbGF5TmFtZSIsDQogICAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTcwNjgwMzYzNzQ1OTQwMTMiDQogICAgICB9LA0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJNYW51ZmFjdHVyZXIiLA0KICAgICAgICAidGFyZ2V0UHJvcGVydHlJZCI6ICIzMTE3MDY4MDMxOTUwMDAwMzMxIg0KICAgICAgfQ0KICAgIF0sDQogICAgInNvdXJjZVRhYmxlUHJvcGVydGllcyI6IHsNCiAgICAgICJzb3VyY2VUeXBlIjogIkxha2Vob3VzZVRhYmxlIiwNCiAgICAgICJ3b3Jrc3BhY2VJZCI6ICI1ODBmNDEwZS03MzNkLTQzYmQtOGE4Ny1iZTEyYjUzNmY3ZmYiLA0KICAgICAgIml0ZW1JZCI6ICJkMGQ4NjNiYy00OGUxLTQ1YjItOGY0Yi01NDc5NWM5N2JhNzEiLA0KICAgICAgInNvdXJjZVRhYmxlTmFtZSI6ICJlcXVpcG1lbnQxbm9udGltZXNlcmllcyIsDQogICAgICAic291cmNlU2NoZW1hIjogImRibyINCiAgICB9DQogIH0NCn0=",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/8813598896083/DataBindings/39a3889b-77e4-4851-8960-e3b779a5d9ba.json",
      "payload": "ew0KICAiaWQiOiAiMzlhMzg4OWItNzdlNC00ODUxLTg5NjAtZTNiNzc5YTVkOWJhIiwNCiAgImRhdGFCaW5kaW5nQ29uZmlndXJhdGlvbiI6IHsNCiAgICAiZGF0YUJpbmRpbmdUeXBlIjogIlRpbWVTZXJpZXMiLA0KICAgICJ0aW1lc3RhbXBDb2x1bW5OYW1lIjogIlByZWNpc2VUaW1lc3RhbXAiLA0KICAgICJwcm9wZXJ0eUJpbmRpbmdzIjogWw0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJQcmVjaXNlVGltZXN0YW1wIiwNCiAgICAgICAgInRhcmdldFByb3BlcnR5SWQiOiAiMzExNDU4NDk4MTM2ODc5Njk1MyINCiAgICAgIH0sDQogICAgICB7DQogICAgICAgICJzb3VyY2VDb2x1bW5OYW1lIjogIk5hbWUiLA0KICAgICAgICAidGFyZ2V0UHJvcGVydHlJZCI6ICIzMTE0NTg0OTc5NzQzMzIwOTM0Ig0KICAgICAgfSwNCiAgICAgIHsNCiAgICAgICAgInNvdXJjZUNvbHVtbk5hbWUiOiAiVGVtcGVyYXR1cmUiLA0KICAgICAgICAidGFyZ2V0UHJvcGVydHlJZCI6ICIzMTE0NTg0OTc3NTYyNjc5NjcyIg0KICAgICAgfSwNCiAgICAgIHsNCiAgICAgICAgInNvdXJjZUNvbHVtbk5hbWUiOiAiTmFtZSIsDQogICAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTcwNjgwMzYzNzQ1OTQwMTMiDQogICAgICB9DQogICAgXSwNCiAgICAic291cmNlVGFibGVQcm9wZXJ0aWVzIjogew0KICAgICAgInNvdXJjZVR5cGUiOiAiTGFrZWhvdXNlVGFibGUiLA0KICAgICAgIndvcmtzcGFjZUlkIjogIjU4MGY0MTBlLTczM2QtNDNiZC04YTg3LWJlMTJiNTM2ZjdmZiIsDQogICAgICAiaXRlbUlkIjogImQwZDg2M2JjLTQ4ZTEtNDViMi04ZjRiLTU0Nzk1Yzk3YmE3MSIsDQogICAgICAic291cmNlVGFibGVOYW1lIjogImVxdWlwbWVudDF0aW1lc2VyaWVzIiwNCiAgICAgICJzb3VyY2VTY2hlbWEiOiAiZGJvIg0KICAgIH0NCiAgfQ0KfQ==",
      "payloadType": "InlineBase64"
    },  
    {
      "path": "EntityTypes/8813598896083/Documents/document1.json",
      "payload": "eyANCiAgImRpc3BsYXlUZXh0IjogImRvYzEiLCANCiAgInVybCI6ICJleGFtcGxldXJsMSIgDQp9",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/8813598896083/Overviews/definition.json",
      "payload": "ew0KICAid2lkZ2V0cyI6IFsNCiAgICB7DQogICAgICAidHlwZSI6ICJsaW5lQ2hhcnQiLA0KICAgICAgInlBeGlzUHJvcGVydHlJZCI6ICIzMTE0NTg0OTc3NTYyNjc5NjcyIiwNCiAgICAgICJpZCI6ICI2ZDczMzAxOC1jMTc3LTQ3YjUtODdiYS02MDI0Y2MxNDM3NzYiLA0KICAgICAgInRpdGxlIjogInRlbXBlcmF0dXJlIGRhdGEiDQogICAgfQ0KICBdLA0KICAic2V0dGluZ3MiOiB7DQogICAgInR5cGUiOiAiZml4ZWRUaW1lIiwNCiAgICAiZml4ZWRUaW1lUmFuZ2UiOiAiTGFzdDFIb3VyIiwNCiAgICAiaW50ZXJ2YWwiOiAiT25lSG91ciIsDQogICAgImFnZ3JlZ2F0aW9uIjogIkF2ZXJhZ2UiDQogIH0NCn0=",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/159990879905613/definition.json",
      "payload": "ew0KICAiaWQiOiAiMTU5OTkwODc5OTA1NjEzIiwNCiAgIm5hbWVzcGFjZSI6ICJ1c2VydHlwZXMiLA0KICAiYmFzZUVudGl0eVR5cGVJZCI6IG51bGwsDQogICJuYW1lIjogIkVxdWlwbWVudDIiLA0KICAiZW50aXR5SWRQYXJ0cyI6IFsNCiAgICAiMzExMzQ5MzI1NjY3NDEyOTE1MSINCiAgXSwNCiAgImRpc3BsYXlOYW1lUHJvcGVydHlJZCI6ICIzMTEzNDkzMjU2Njc0MTI5MTUxIiwNCiAgIm5hbWVzcGFjZVR5cGUiOiAiQ3VzdG9tIiwNCiAgInZpc2liaWxpdHkiOiAiVmlzaWJsZSIsDQogICJwcm9wZXJ0aWVzIjogWw0KICAgIHsNCiAgICAgICJpZCI6ICIzMTEzNDkzMjU2Njc0MTI5MTUxIiwNCiAgICAgICJuYW1lIjogIkRpc3BsYXlOYW1lIiwNCiAgICAgICJyZWRlZmluZXMiOiBudWxsLA0KICAgICAgImJhc2VUeXBlTmFtZXNwYWNlVHlwZSI6IG51bGwsDQogICAgICAidmFsdWVUeXBlIjogIlN0cmluZyINCiAgICB9LA0KICAgIHsNCiAgICAgICJpZCI6ICIzMTEzNDkzMjUzMTc3MDYyMTY4IiwNCiAgICAgICJuYW1lIjogIk1hbnVmYWN0dXJlciIsDQogICAgICAicmVkZWZpbmVzIjogbnVsbCwNCiAgICAgICJiYXNlVHlwZU5hbWVzcGFjZVR5cGUiOiBudWxsLA0KICAgICAgInZhbHVlVHlwZSI6ICJTdHJpbmciDQogICAgfQ0KICBdLA0KICAidGltZXNlcmllc1Byb3BlcnRpZXMiOiBbDQogICAgew0KICAgICAgImlkIjogIjMxMTE4Nzg1NDU4NTQ3MDE1MjUiLA0KICAgICAgIm5hbWUiOiAiUHJlY2lzZVRpbWVzdGFtcCIsDQogICAgICAicmVkZWZpbmVzIjogbnVsbCwNCiAgICAgICJiYXNlVHlwZU5hbWVzcGFjZVR5cGUiOiBudWxsLA0KICAgICAgInZhbHVlVHlwZSI6ICJEYXRlVGltZSINCiAgICB9LA0KICAgIHsNCiAgICAgICJpZCI6ICIzMTExODc4NTQ2MTUwMzUzMzEyIiwNCiAgICAgICJuYW1lIjogIk5hbWUiLA0KICAgICAgInJlZGVmaW5lcyI6IG51bGwsDQogICAgICAiYmFzZVR5cGVOYW1lc3BhY2VUeXBlIjogbnVsbCwNCiAgICAgICJ2YWx1ZVR5cGUiOiAiU3RyaW5nIg0KICAgIH0sDQogICAgew0KICAgICAgImlkIjogIjMxMTE4Nzg1NDQwMTM1MzYzNTIiLA0KICAgICAgIm5hbWUiOiAiVGVtcGVyYXR1cmUiLA0KICAgICAgInJlZGVmaW5lcyI6IG51bGwsDQogICAgICAiYmFzZVR5cGVOYW1lc3BhY2VUeXBlIjogbnVsbCwNCiAgICAgICJ2YWx1ZVR5cGUiOiAiRG91YmxlIg0KICAgIH0NCiAgXQ0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/159990879905613/DataBindings/60efb0e8-2225-4a1b-84c8-1987d37ee422.json",
      "payload": "ew0KICAiaWQiOiAiNjBlZmIwZTgtMjIyNS00YTFiLTg0YzgtMTk4N2QzN2VlNDIyIiwNCiAgImRhdGFCaW5kaW5nQ29uZmlndXJhdGlvbiI6IHsNCiAgICAiZGF0YUJpbmRpbmdUeXBlIjogIk5vblRpbWVTZXJpZXMiLA0KICAgICJwcm9wZXJ0eUJpbmRpbmdzIjogWw0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJEaXNwbGF5TmFtZSIsDQogICAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTM0OTMyNTY2NzQxMjkxNTEiDQogICAgICB9LA0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJNYW51ZmFjdHVyZXIiLA0KICAgICAgICAidGFyZ2V0UHJvcGVydHlJZCI6ICIzMTEzNDkzMjUzMTc3MDYyMTY4Ig0KICAgICAgfQ0KICAgIF0sDQogICAgInNvdXJjZVRhYmxlUHJvcGVydGllcyI6IHsNCiAgICAgICJzb3VyY2VUeXBlIjogIkxha2Vob3VzZVRhYmxlIiwNCiAgICAgICJ3b3Jrc3BhY2VJZCI6ICI1ODBmNDEwZS03MzNkLTQzYmQtOGE4Ny1iZTEyYjUzNmY3ZmYiLA0KICAgICAgIml0ZW1JZCI6ICJkMGQ4NjNiYy00OGUxLTQ1YjItOGY0Yi01NDc5NWM5N2JhNzEiLA0KICAgICAgInNvdXJjZVRhYmxlTmFtZSI6ICJlcXVpcG1lbnQybm9udGltZXNlcmllcyIsDQogICAgICAic291cmNlU2NoZW1hIjogImRibyINCiAgICB9DQogIH0NCn0=",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/159990879905613/DataBindings/0542df13-5b4b-4590-8dcf-ea2435d90a73.json",
      "payload": "ew0KICAiaWQiOiAiMDU0MmRmMTMtNWI0Yi00NTkwLThkY2YtZWEyNDM1ZDkwYTczIiwNCiAgImRhdGFCaW5kaW5nQ29uZmlndXJhdGlvbiI6IHsNCiAgICAiZGF0YUJpbmRpbmdUeXBlIjogIlRpbWVTZXJpZXMiLA0KICAgICJ0aW1lc3RhbXBDb2x1bW5OYW1lIjogIlByZWNpc2VUaW1lc3RhbXAiLA0KICAgICJwcm9wZXJ0eUJpbmRpbmdzIjogWw0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJOYW1lIiwNCiAgICAgICAgInRhcmdldFByb3BlcnR5SWQiOiAiMzExMzQ5MzI1NjY3NDEyOTE1MSINCiAgICAgIH0sDQogICAgICB7DQogICAgICAgICJzb3VyY2VDb2x1bW5OYW1lIjogIk5hbWUiLA0KICAgICAgICAidGFyZ2V0UHJvcGVydHlJZCI6ICIzMTExODc4NTQ2MTUwMzUzMzEyIg0KICAgICAgfSwNCiAgICAgIHsNCiAgICAgICAgInNvdXJjZUNvbHVtbk5hbWUiOiAiUHJlY2lzZVRpbWVzdGFtcCIsDQogICAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTE4Nzg1NDU4NTQ3MDE1MjUiDQogICAgICB9LA0KICAgICAgew0KICAgICAgICAic291cmNlQ29sdW1uTmFtZSI6ICJUZW1wZXJhdHVyZSIsDQogICAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTE4Nzg1NDQwMTM1MzYzNTIiDQogICAgICB9DQogICAgXSwNCiAgICAic291cmNlVGFibGVQcm9wZXJ0aWVzIjogew0KICAgICAgInNvdXJjZVR5cGUiOiAiTGFrZWhvdXNlVGFibGUiLA0KICAgICAgIndvcmtzcGFjZUlkIjogIjU4MGY0MTBlLTczM2QtNDNiZC04YTg3LWJlMTJiNTM2ZjdmZiIsDQogICAgICAiaXRlbUlkIjogImQwZDg2M2JjLTQ4ZTEtNDViMi04ZjRiLTU0Nzk1Yzk3YmE3MSIsDQogICAgICAic291cmNlVGFibGVOYW1lIjogImVxdWlwbWVudDJ0aW1lc2VyaWVzIiwNCiAgICAgICJzb3VyY2VTY2hlbWEiOiAiZGJvIg0KICAgIH0NCiAgfQ0KfQ== ",
      "payloadType": "InlineBase64"
    },
    {
      "path": "RelationshipTypes/3110733855942077719/definition.json",
      "payload": "ew0KICAibmFtZXNwYWNlIjogInVzZXJ0eXBlcyIsDQogICJpZCI6ICIzMTEwNzMzODU1OTQyMDc3NzE5IiwNCiAgIm5hbWUiOiAiY29udGFpbnMiLA0KICAibmFtZXNwYWNlVHlwZSI6ICJDdXN0b20iLA0KICAic291cmNlIjogew0KICAgICJlbnRpdHlUeXBlSWQiOiAiODgxMzU5ODg5NjA4MyINCiAgfSwNCiAgInRhcmdldCI6IHsNCiAgICAiZW50aXR5VHlwZUlkIjogIjE1OTk5MDg3OTkwNTYxMyINCiAgfQ0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "RelationshipTypes/3110733855942077719/Contextualizations/62bbbf52-39a4-47ed-b7bf-651debaca6ab.json",
      "payload": "ew0KICAiaWQiOiAiNjJiYmJmNTItMzlhNC00N2VkLWI3YmYtNjUxZGViYWNhNmFiIiwNCiAgImRhdGFCaW5kaW5nVGFibGUiOiB7DQogICAgIndvcmtzcGFjZUlkIjogIjU4MGY0MTBlLTczM2QtNDNiZC04YTg3LWJlMTJiNTM2ZjdmZiIsDQogICAgIml0ZW1JZCI6ICJkMGQ4NjNiYy00OGUxLTQ1YjItOGY0Yi01NDc5NWM5N2JhNzEiLA0KICAgICJzb3VyY2VUYWJsZU5hbWUiOiAicmVsYXRpb25zaGlwdGFibGUiLA0KICAgICJzb3VyY2VTY2hlbWEiOiAiZGJvIiwNCiAgICAic291cmNlVHlwZSI6ICJMYWtlaG91c2VUYWJsZSINCiAgfSwNCiAgInNvdXJjZUtleVJlZkJpbmRpbmdzIjogWw0KICAgIHsNCiAgICAgICJzb3VyY2VDb2x1bW5OYW1lIjogIkVxdWlwbWVudDFOYW1lIiwNCiAgICAgICJ0YXJnZXRQcm9wZXJ0eUlkIjogIjMxMTcwNjgwMzYzNzQ1OTQwMTMiDQogICAgfQ0KICBdLA0KICAidGFyZ2V0S2V5UmVmQmluZGluZ3MiOiBbDQogICAgew0KICAgICAgInNvdXJjZUNvbHVtbk5hbWUiOiAiRXF1aXBtZW50Mk5hbWUiLA0KICAgICAgInRhcmdldFByb3BlcnR5SWQiOiAiMzExMzQ5MzI1NjY3NDEyOTE1MSINCiAgICB9DQogIF0NCn0=",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## DefinitionDetails 

The DefinitionDetails file name is *definition.json*. It is an empty definition.

### DefinitionDetails file example 

```json
{}
```

## EntityTypes/{ID} directory: EntityType file

The EntityType file name is the *definition.json*.

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | BigInt | true | Unique ID of the entity type. The entity type ID is a positive 64-bit integer that's unique across the ontology instance. |
| `namespace` | string | true | Allowed value: *usertypes*. |
| `baseEntityTypeId` | BigInt | false | Unique ID of the base entity type. |
| `name` | string | true | Name of the entity type. Must follow this Regex pattern: `^[a-zA-Z][a-zA-Z0-9\_-]{0,127}\$`. |
| `entityIdParts` | BigInt[] | false | The properties (by ID) that together uniquely identify an entity. |
| `displayNamePropertyId` | BigInt | false | The ID of the property that's used as the display name for the entity. |
| `namespaceType` | string | true | Allowed value: *Custom.* |
| `visibility` | string | false | Allowed value: *Visible.* |
| `properties` | EntityTypeProperty[] | false | List of entity type properties. |
| `timeseriesProperties` | EntityTypeProperty[] | false | List of entity type properties. |

### EntityTypeProperty

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | BigInt | true | Unique ID of the entity type property. |
| `name` | string | true | Name of the entity type property. Must follow this Regex pattern: `^[a-zA-Z][a-zA-Z0-9\_-]{0,127}\$`. |
| `redefines` | string | false | Pointer to property inherited from base type that this property redefines. |
| `baseTypeNamespaceType` | string | false | The namespace of the base entity type. |
| `valueType` | string | true | Describes the value type of the entity type property. Allowed values: *String*, *Boolean*, *DateTime*, *Object*, *BigInt*, *Double*. |

### EntityType file example

```json
{
  "id": "8813598896083",
  "namespace": "usertypes",
  "baseEntityTypeId": null,
  "name": "Equipment1",
  "entityIdParts": [
    "3117068036374594013"
  ],
  "displayNamePropertyId": "3117068036374594013",
  "namespaceType": "Custom",
  "visibility": "Visible",
  "properties": [
    {
      "id": "3117068036374594013",
      "name": "DisplayName",
      "redefines": null,
      "baseTypeNamespaceType": null,
      "valueType": "String"
    },
    {
      "id": "3117068031950000331",
      "name": "Manufacturer",
      "redefines": null,
      "baseTypeNamespaceType": null,
      "valueType": "String"
    }
  ],
  "timeseriesProperties": [
    {
      "id": "3114584981368796953",
      "name": "PreciseTimestamp",
      "redefines": null,
      "baseTypeNamespaceType": null,
      "valueType": "DateTime"
    },
    {
      "id": "3114584979743320934",
      "name": "Name",
      "redefines": null,
      "baseTypeNamespaceType": null,
      "valueType": "String"
    },
    {
      "id": "3114584977562679672",
      "name": "Temperature",
      "redefines": null,
      "baseTypeNamespaceType": null,
      "valueType": "Double"
    }
  ]
}
```

## EntityTypes/{ID}/DataBindings directory: DataBinding file

The DataBinding file name is the DataBinding ID.

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | Guid | true | Unique ID of the data binding. |
| `dataBindingConfiguration` | DataBindingConfiguration | true | Configuration specific to the kind of data binding. |

### DataBindingConfiguration

| Property | Type | Required | Description |
|----|----|----|----|
| `dataBindingType` | string | true | Allowed values: *TimeSeries*, *NonTimeSeries* |
| `timestampColumnName` | string | false | Only required if `dataBindingType` is *TimeSeries*. This is the name of the timestamp column from the data table. |
| `propertyBindings` | EntityTypePropertyBinding[] | false | The bindings from source columns to entity type properties. |
| `sourceTableProperties` | LakehouseTableDataBindingProperties or EventhouseTableDataBindingProperties.<br>**Note:** EventhouseTableDataBindingProperties can only be used if the `dataBindingType` is *TimeSeries*. | true | Data source table properties. |

### EntityTypePropertyBinding

| Property | Type | Required | Description |
|----|----|----|----|
| `sourceColumnName` | string | true | The name of the source column in the data table. |
| `targetPropertyId` | string | true | The target property ID in the entity type. |

### LakehouseTableDataBindingProperties

| Property | Type | Required | Description |
|----|----|----|----|
| `sourceType` | string | true | The sourceType of the Data Binding. Allowed value: *LakehouseTable*. |
| `workspaceId` | Guid | true | The ID of the workspace where the customer's lakehouse table is located. |
| `itemId` | Guid | true | `ArtifactId` of the customer's lakehouse. |
| `sourceTableName` | string | true | The name of the data table from the lakehouse. |
| `sourceSchema` | string | false | The schema of the data table. |

### EventhouseTableDataBindingProperties

| Property | Type | Required | Description |
|----|----|----|----|
| `sourceType` | string | true | The source type of the Data Binding. Allowed value: *KustoTable*. |
| `workspaceId` | Guid | true | The ID of the workspace where the customer's eventhouse is located. |
| `itemId` | Guid | true | `ArtifactId` of the customer's eventhouse. |
| `clusterUri` | string | true | The URL to the Kusto cluster. |
| `databaseName` | string | true | The name of the database in the Kusto cluster. |
| `sourceTableName` | string | true | The name of the source table in the Kusto cluster. |

### DataBinding file example

```json
{
  "id": "39a3889b-77e4-4851-8960-e3b779a5d9ba",
  "dataBindingConfiguration": {
    "dataBindingType": "TimeSeries",
    "timestampColumnName": "PreciseTimestamp",
    "propertyBindings": [
      {
          "sourceColumnName": "PreciseTimestamp",
          "targetPropertyId": "3114584981368796953"
      },
      {
          "sourceColumnName": "Name",
          "targetPropertyId": "3114584979743320934"
      },
      {
          "sourceColumnName": "Temperature",
          "targetPropertyId": "3114584977562679672"
      },
      {
          "sourceColumnName": "Name",
          "targetPropertyId": "3117068036374594013"
      }
    ],
    "sourceTableProperties": {
      "sourceType": "LakehouseTable",
      "workspaceId": "580f410e-733d-43bd-8a87-be12b536f7ff",
      "itemId": "d0d863bc-48e1-45b2-8f4b-54795c97ba71",
      "sourceTableName": "equipment1timeseries",
      "sourceSchema": "dbo"
    }
  }
}
```

## EntityTypes/{ID}/Documents directory: Document file

The Document file name is the *document{id}.json*, where *id* is the document number for the entity type.

### Document

| Property    | Type   | Required | Description                        |
|-------------|--------|----------|------------------------------------|
| `displayText` | string | false    | The display text for the document. |
| `url`         | string | true     | The URL pointing to the document.  |

### Document file example

```json
{
  "displayText": "doc1",
  "url": "exampleurl1"
}
```

## EntityTypes/{ID}/Overviews directory: Overviews file

The Overviews file name is *definition.json*.

### Overviews

| Property | Type                | Required | Description                    |
|----------|---------------------|----------|--------------------------------|
| `widgets`  | Widget[] | false    | The widgets on the preview experience.  |
| `settings` | Settings   | false    | The settings of the preview experience. |

### Widget

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | Guid | true | Unique ID of the widget. |
| `type` | string | true | Type of widget. Allowed values: *lineChart*, *barChart*, *file*, *graph*, *liveMap*. |
| `title` | string | false | The title of the widget. |
| `yAxisPropertyId` | string | false | The property to display on the y-axis.<br>**Note:** Include this field only if `type` is *lineChart* or *barChart*. |

### Settings

| Property | Type | Required | Description |
|----|----|----|----|
| `type` | string | true | Type of Settings. Allowed values: *fixedTime*, *customTime*. |
| `interval` | string | true | Sample window values in seconds for the preview experience data. Allowed values: *OneMinute*, *FiveMinutes*, *FifteenMinutes*, *ThirtyMinutes*, *OneHour*, *SixHours*, *TwelveHours*, *OneDay*. |
| `aggregation` | string | true | Aggregation function for the preview experience data. Allowed values: *Average*, *Count*, *Maximum*, *Minimum*, *Sum*, *LastKnownValue*. |
| `fixedTimeRange` | string | false | The fixed time range for the preview experience data. Allowed values: *Last30Minutes*, *Last1Hour*, *Last4Hours*, *Last12Hours*, *Last24Hours*, *Last48Hours*, *Last3Days*, *Last7Days*, *Last30Days*.<br>**Note:** Include this field only if `type` is *fixedTime*. |
| `timeRange` | TimeRange | false | Time range for the preview experience data.<br>**Note:** Include this field only if `type` is *customTime*. |

### TimeRange

| Property  | Type        | Required | Description                           |
|-----------|-------------|----------|---------------------------------------|
| `startTime` | UTCDateTime | true     | Start time for the custom time range. |
| `endTime`   | UTCDateTime | true     | End time for the custom time range.   |

### Overviews file example

```json
{
  "widgets": [
    {
      "type": "lineChart",
      "yAxisPropertyId": "3114584977562679672",
      "id": "6d733018-c177-47b5-87ba-6024cc143776",
      "title": "temperature data"
    }
  ],
  "settings": {
    "type": "fixedTime",
    "fixedTimeRange": "Last1Hour",
    "interval": "OneHour",
    "aggregation": "Average"
  }
}
```

## RelationshipTypes/{ID} directory: RelationshipType file

The RelationshipType file name is *definition.json*.

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | BigInt | true | Unique ID of the relationship type. |
| `namespace` | string | true | Allowed value: *usertypes*. |
| `name` | string | true | Name of the relationship type. Must follow this Regex pattern: `^[a-zA-Z][a-zA-Z0-9\_-]{0,127}\$`. |
| `namespaceType` | BigInt | true | Allowed value: *Custom*. |
| `source` | RelationshipEnd | true | The relationship end that denotes the source. |
| `target` | RelationshipEnd | true | The relationship end that denotes the target. |

### RelationshipEnd

| Property | Type | Required | Description |
|----|----|----|----|
| `entityTypeId` | string | true | The entity type ID of the relationship end that exists in the workspace. The entity type ID is a positive 64-bit integer that's unique across the ontology instance. |

### RelationshipType file example

```json
{
  "namespace": "usertypes",
  "id": "3110733855942077719",
  "name": "contains",
  "namespaceType": "Custom",
  "source": {
    "entityTypeId": "8813598896083"
  },
  "target": {
    "entityTypeId": "159990879905613"
  }
}
```

## RelationshipTypes/{ID}/Contextualizations directory: Contextualization file

The Contextualization file name is the contextualization ID.

| Property | Type | Required | Description |
|----|----|----|----|
| `id` | Guid | true | Unique ID of the contextualization. |
| `dataBindingTable` | LakehouseTableDataBindingProperties | true | The source lakehouse data table. |
| `sourceKeyRefBindings` | EntityTypePropertyBinding[] | true | The columns in the dataTable making up the unique ID of the source-side entity type. |
| `targetKeyRefBindings` | EntityTypePropertyBinding[] | true | The columns in the dataTable making up the unique ID of the target-side entity type. |

### Contextualization file example:

```json
{
  "id": "62bbbf52-39a4-47ed-b7bf-651debaca6ab",
  "dataBindingTable": {
    "workspaceId": "580f410e-733d-43bd-8a87-be12b536f7ff",
    "itemId": "d0d863bc-48e1-45b2-8f4b-54795c97ba71",
    "sourceTableName": "relationshiptable",
    "sourceSchema": "dbo",
    "sourceType": "LakehouseTable"
  },
  "sourceKeyRefBindings": [
    {
      "sourceColumnName": "Equipment1Name",
      "targetPropertyId": "3117068036374594013"
    }
  ],
  "targetKeyRefBindings": [
    {
      "sourceColumnName": "Equipment2Name",
      "targetPropertyId": "3113493256674129151"
    }
  ]
}
```

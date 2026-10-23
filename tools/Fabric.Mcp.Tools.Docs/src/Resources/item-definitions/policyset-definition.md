# PolicySet definition

This article provides a breakdown of the structure for PolicySet definition items.

## Supported formats

PolicySet items support the `Beta` definition format. Specify `Beta` in the `format` property of the item definition, as shown in the [definition example](#definition-example).

The `policySet.json` definition part contains JSON content. JSON describes the part's content, not the value of the definition's `format` property.

## Definition parts

This table lists the PolicySet definition parts for the `Beta` format.

| Definition part path | Type | Required | Description |
| --- | --- | --- | --- |
| `policySet.json` | [ContentDetails](#contentdetails) (JSON) | true | Describes the content of the payload |
| `.platform` | [PlatformDetails](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/item-definition-overview#platform-file) (JSON) | false | Describes common details of the item |

## Definition example

```json
{
  "definition": {
    "parts": [
      {
        "path": "policySet.json",
        "payload": "eyJwcm9wZXJ0aWVzIjp7InNjb3BlIjp7InR5cGUiOiJDYXBhY2l0eSJ9fSwicG9saWN5UnVsZXMiOltdfQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
        "payloadType": "InlineBase64"
      }
    ],
    "format": "Beta"
  }
}
```

## ContentDetails

Schema version: `1.0.0`. Source: [schema.json](https://github.com/microsoft/json-schemas/blob/main/fabric/item/policySet/definition/1.0.0/schema.json). The schema uses JSON Schema draft-07.

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `$schema` | string | true | Defines the schema to use for a policy set definition. Must be the URI `https://developer.microsoft.com/json-schemas/fabric/item/policySet/definition/1.0.0/schema.json`. |
| `properties` | [object](#properties) | true | Defines the properties of the policy set. See the nested properties below. |
| `policyRules` | [PolicyRule](#policyrule)[] | true | Defines the rules in the policy set. |

The root object and every object explicitly defined in this schema disallow additional properties. The arrays `policyRules`, `conditions`, `effects`, and `values` have no minimum or maximum item counts and no uniqueness constraint in this schema. Required arrays must be present, but may be empty under schema validation; platform requirements might be stricter.

### properties

This inline object is the root-level `properties` value, not a named schema definition.

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `scope` | [object](#propertiesscope) | true | Defines the scope of the policy set. |

### properties.scope

This inline object is the `properties.scope` value and specifies the scope of the policy set.

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | string | true | The scope type. Supported values and length restrictions are defined by the policy platform rather than constrained by this schema. |

### PolicyRule

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `displayName` | string | true | The display name of the policy rule. Free-form text with no length restrictions imposed by this schema. |
| `description` | string | false | An optional description of the policy rule. Free-form text with no length restrictions imposed by this schema. |
| `policy` | string (ExternalDataSharing, ItemCreation) | true | The policy type governed by the rule. |
| `conditions` | [Condition](#condition)[] | true | Defines the conditions of the policy rule. Each item must match exactly one condition variant. |
| `effects` | [Effect](#effect)[] | true | Defines the effects of the policy rule. |

### Condition

This is a complex `oneOf` definition with effective type `object`. It does not declare properties or a required array directly. Each condition must match exactly one of the following definitions; required properties are specified by the selected definition.

| Type | Description |
| --- | --- |
| [StaticCondition](#staticcondition) | A static condition whose `type` must be `Static`. |
| [DynamicCondition](#dynamiccondition) | A dynamic condition whose `type` must be `Dynamic`, with a required target property and predicate. |

### StaticCondition

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | string | true | Identifies a static condition. Only `Static` is permitted. |

### DynamicCondition

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | string | true | Identifies a dynamic condition. Only `Dynamic` is permitted. |
| `targetProperty` | string | true | The property evaluated by the condition, expressed as an identifier or a dot-separated property path. Each segment must start with an ASCII letter or underscore, followed by ASCII letters, digits, or underscores. |
| `predicate` | [Predicate](#predicate) | true | Defines the comparison applied to the target property. |

The `targetProperty` pattern is `^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$`. For example, `item.type` matches this pattern. Matching the pattern does not establish that a property is supported by the policy platform.

### Predicate

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `operator` | string (AnyOf, NoneOf) | true | The comparison operator applied to the target property and the supplied values. |
| `values` | string[] | true | The values used for comparison with the target property. Each item is a comparison value. Allowed values and length restrictions depend on the target property and are not constrained by this schema. |

### Effect

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | string | true | Identifies the effect of the policy rule. Only `Allow` is permitted. |

### PolicySet file example

This example illustrates a dynamic condition using one rule, with a `Tenant` scope and a comparison against the `Lakehouse` item type. Rule evaluation behavior is not specified by this schema.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/policySet/definition/1.0.0/schema.json",
  "properties": {
    "scope": {
      "type": "Tenant"
    }
  },
  "policyRules": [
    {
      "displayName": "External data sharing dynamic rule",
      "description": "Illustrates a property comparison with an Allow effect.",
      "policy": "ExternalDataSharing",
      "conditions": [
        {
          "type": "Dynamic",
          "targetProperty": "item.type",
          "predicate": {
            "operator": "AnyOf",
            "values": [
              "Lakehouse"
            ]
          }
        }
      ],
      "effects": [
        {
          "type": "Allow"
        }
      ]
    }
  ]
}
```

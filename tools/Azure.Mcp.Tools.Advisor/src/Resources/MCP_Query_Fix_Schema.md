# Query

## Structure

```
"ARM_Query": 
{
  "allOf" | "anyOf":  // AND / OR over conditions
  [
    {
      "field": "dot.path", 
      "<operator>": <value> 
    }
  ]
}
```

## Operators Supported

| Operator                   | Meaning                                  |
|----------------------------|------------------------------------------|
| equals / notEquals         | Exact match (strings, booleans)          |
| contains / notContains     | Substring match                          |
| startsWith                 | Prefix match                             |
| exists                     | true / false — property presence check   |
| in / notIn                 | Value is in a list                       |
| less / lessOrEquals        | Numeric comparison                       |
| greater / greaterOrEquals  | Numeric comparison                       |


# Fix

## Structure

```
// Type 1: direct ARM template property patch
{ 
  "type": "propertyUpdate",
  "operations": 
  [{ "field": "dot.path", "value": <new_value> }] 
}

// Type 2: action that requires more than a property set
{ 
  "type": "resourceAction",
  "action": "migrate|retire|resize|createAndLink|configure",
  "targetService": "<ARM resource type>",   // optional
  "details": "What to do" 
}
```

## Operators Supported

| action          | Used for                                               | Examples                                                      |
|-----------------|--------------------------------------------------------|---------------------------------------------------------------|
| propertyUpdate  | Single flag or setting change possible in-place        | TLS versions, soft-delete, zone balance, CSI drivers, capacity|
| configure       | Multi-step or conditional array manipulation           | WAF ruleset upgrades, APIM additional locations               |
| migrate         | Move to a different service or platform                | stv1→stv2, LUIS→CLU, AKS on Azure Local                      |
| retire          | Resource type is retired, must be removed              | AI Health Insights, QnA Maker, SCOM MI                       |
| resize          | VM SKU/size change requiring deallocation              | A/B-series, retiring F/G/Ls, NC24rs_v3                       |
| createAndLink   | Must create a separate resource and reference it       | Capacity Reservation Group, NetApp snapshot policy           |
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


---

# Terraform Query

## Structure

```
"TF_Query":
{
  "resource_type": "azurerm_xxx",   // AzureRM Terraform provider resource type
  "allOf" | "anyOf":  // AND / OR over conditions
  [
    {
      "attribute": "hcl.attribute.path",
      "<operator>": <value>
    }
  ]
}
```

## Attribute Path Convention

- Use dot notation for nested blocks (e.g., `storage_profile.disk_driver_enabled`)
- Use `[N]` for indexed repeated blocks (e.g., `managed_rule_set[0].version`)
- Top-level attributes use exact Terraform schema names (snake_case)

## Operators Supported

Same operators as ARM_Query: equals / notEquals, contains / notContains, startsWith, exists, in / notIn, less / lessOrEquals, greater / greaterOrEquals


# Terraform Fix

## Structure

```
// Type 1: direct Terraform attribute patch
{
  "type": "attributeUpdate",
  "operations":
  [{ "attribute": "hcl.attribute.path", "value": <new_value> }]
}

// Type 2: action that requires more than an attribute set
{
  "type": "resourceAction",
  "action": "migrate|retire|resize|createAndLink|configure",
  "targetResource": "<azurerm_resource_type>",   // optional
  "details": "What to do in Terraform/HCL terms"
}
```

## Actions Supported

| action          | Used for                                               | Examples                                                              |
|-----------------|--------------------------------------------------------|-----------------------------------------------------------------------|
| attributeUpdate | Single flag or setting change possible in-place        | TLS versions, soft_delete_retention_days, CSI drivers, worker_count  |
| configure       | Multi-step or conditional block manipulation           | WAF ruleset upgrades, APIM additional_location blocks                 |
| migrate         | Move to a different resource type or platform          | stv1→stv2, LUIS→TextAnalytics, Arc App Service→Container Apps        |
| retire          | Resource type is retired, must be removed              | AI Health Insights, QnA Maker                                         |
| resize          | VM size change requiring deallocation                  | A/B-series, retiring F/G/Ls, NC24rs_v3                               |
| createAndLink   | Must create a separate resource and reference it       | azurerm_capacity_reservation_group, azurerm_netapp_snapshot_policy   |
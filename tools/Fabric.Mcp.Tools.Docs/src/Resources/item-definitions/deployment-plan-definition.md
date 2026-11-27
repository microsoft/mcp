# Deployment plan definition

This article provides a breakdown of the definition structure for deployment plan items. A deployment plan controls the order in which items are deployed and defines actions to run before or after item deployment.

## Definition parts

This table lists the deployment plan definition parts.

| Definition part path | Type | Required | Description |
|--|--|--|--|
| `plan.yml` | DeploymentPlan (YAML) | &#9989; | Describes the deployment plan. |
| `.platform` | PlatformDetails (JSON) | &#10060; | Describes the metadata of the item. |

## DeploymentPlan

Describes a deployment plan.

| Name | Type | Required | Description |
|---|---|---|---|
| `$schema` | String | &#9989; | The JSON schema URL for the deployment plan definition. |
| `version` | String | &#9989; | The deployment plan definition version. |
| `groups` | [DeploymentGroup](#deploymentgroup)[] | &#9989; | The collection of deployment groups. |

### DeploymentGroup

Describes a deployment group that represents the item identified by `logicalId`.

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | String | &#9989; | The name of the deployment group. |
| `logicalId` | String (Guid) | &#9989; | The logical ID of the deployed item. |
| `dependsOn` | [GroupDependency](#groupdependency)[] | &#10060; | The deployment group dependencies. |
| `preActions` | [DeploymentAction](#deploymentaction)[] | &#10060; | The actions to run before the item is deployed. |
| `postActions` | [DeploymentAction](#deploymentaction)[] | &#10060; | The actions to run after the item is deployed. |

### GroupDependency

Describes a deployment group dependency.

| Name | Type | Required | Description |
|---|---|---|---|
| `groupName` | String | &#9989; | The name of a group that this group depends on. |

### DeploymentAction

Describes a deployment action.

| Name | Type | Required | Description |
|---|---|---|---|
| `name` | String | &#9989; | The name of the action. |
| `job` | [DeploymentJob](#deploymentjob) | &#9989; | The job to run for the action. |

### DeploymentJob

Describes a deployment job.

| Name | Type | Required | Description |
|---|---|---|---|
| `type` | String | &#9989; | The public-facing job type supported by the item referenced by `logicalId`. For standard item execution, use `Execute`. The service resolves it to the item type's internal job implementation. An unsupported job type causes deployment execution to fail. For supported item types, see [Item types you can run as an action](https://learn.microsoft.com/fabric/cicd/deployment-plan/deployment-plan-actions#item-types-you-can-run-as-an-action). |
| `logicalId` | String (Guid) | &#9989; | The logical ID of the item whose job is run. |

### plan.yml example

```YAML
$schema: https://developer.microsoft.com/json-schemas/fabric/item/deploymentPlan/definition/plan/1.0.0/schema.json
version: 1.0.0
groups:
  - name: group1
    logicalId: aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb
    preActions:
      - name: preAction1
        job:
          type: Execute
          logicalId: bbbbbbbb-1111-2222-3333-cccccccccccc
    postActions:
      - name: postAction1
        job:
          type: Execute
          logicalId: dddddddd-3333-4444-5555-eeeeeeeeeeee
  - name: group2
    logicalId: cccccccc-2222-3333-4444-dddddddddddd
    dependsOn:
      - groupName: group1
```

### Considerations and limitations

- Only the item types listed in [Item types you can run as an action](https://learn.microsoft.com/fabric/cicd/deployment-plan/deployment-plan-actions#item-types-you-can-run-as-an-action) can be referenced by `preActions` and `postActions`.
- The `groupName` in a group dependency must match the `name` of another group in the same `plan.yml` file.

## Definition example

```json
{
  "parts": [
    {
      "path": "plan.yml",
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

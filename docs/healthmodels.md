# Azure Monitor Health Models

Three commands read health models. `healthmodels list` and `healthmodels get` work on the model as an Azure resource:
its name, resource group, location, provisioning state, and overall health. `healthmodels inspect` reads what is
inside one model: its entities, their signals, the dependency graph, and history.

## Inspect a model

`inspect` returns CloudHealth API objects as the API returns them (api-version 2026-11-01), passed through unchanged,
so their fields match the [CloudHealth REST API](https://github.com/Azure/azure-rest-api-specs/blob/main/specification/cloudhealth/resource-manager/Microsoft.CloudHealth/CloudHealth/stable/2026-11-01/cloudhealth.json).
Each selected entity is one item:

| Item field | What it holds |
|---|---|
| `entity` | The entity resource |
| `history` | The `getHistory` response, with `--include History` |
| `signalHistory` | One `getSignalHistory` response per signal, with `--include SignalHistory` |
| `annotations` | The `getDataAnnotations` response, with `--include Annotations` |
| `path` | The entity names from a walk's start; the tool's own field |
| `errors` | Reads for this entity that failed; the tool's own field |

`signalDefinitions` next to the items holds the signal definitions the returned signals name, with `--include Signals`.

Every entity that is not healthy, with its signal groups:

```bash
azmcp monitor healthmodels inspect --subscription <subscription> --resource-group <resource-group> \
  --health-model shop --where-health NotHealthy --include Signals
```

Why the model is degraded: walk down from the root entity (named like the model) through the children that drive each
state: Degraded or Unhealthy and not suppressed, and Unknown only where the rollup counts Unknown children (an Unknown
parent, or a `MinHealthy` or `MaxNotHealthy` rollup with `ignoreUnknown: false`):

```bash
azmcp monitor healthmodels inspect ... --health-model shop --walk Causes --include Signals
```

What depends on an entity, and its health history since a date:

```bash
azmcp monitor healthmodels inspect ... --health-model shop --entity db --walk Dependents
azmcp monitor healthmodels inspect ... --health-model shop --entity db --include History --start-at 2026-10-25T00:00:00Z
```

The model as it was at an instant: entities and relationships at that timestamp, and history windows that end there:

```bash
azmcp monitor healthmodels inspect ... --health-model shop --walk Causes --include Signals --timestamp 2026-10-30T12:00:00Z
```

## Choose fields

Without `--fields`, an entity keeps its name, display name, health state, impact, health objective, discovery rule,
provisioning state, tags, signal aggregation groups and dependency rollup (`properties.signalGroups.dependencies`);
`--include Signals` adds its signal groups. `--fields` keeps only the field paths you name, prefixed by item field, in
place and with their API names:

```bash
azmcp monitor healthmodels inspect ... --health-model shop --include Signals \
  --fields entity.properties.healthState,entity.properties.signalGroups.*.signals.status
```

Paths are comma-separated or repeated. A path keeps everything below it, array elements are walked through, `*` as a
segment stands for any one property at its level, `**` for any number of levels (so `entity.**.status` keeps every
`status` in the entity), and `*` alone keeps everything. A path that matches nothing leaves nothing behind. With
`--fields`, the result's `fields` lists every path the payloads hold.

## Options

| Option | Values |
|---|---|
| `--entity` | The entity to read or to start a walk from; a walk defaults to the root entity |
| `--where-health` | `Healthy`, `Degraded`, `Unhealthy`, `Unknown`, `NotHealthy` (any of the last three) |
| `--walk` | `Causes` (down through the children that drive each state), `Dependencies` (everything below), `Dependents` (everything above) |
| `--include` | `Signals`, `History`, `SignalHistory` (needs `--entity`), `Annotations` |
| `--start-at`, `--end-at` | The API's `startAt` and `endAt` for history, signal history and annotations; a given end starts a day earlier |
| `--timestamp` | The API's `timestamp`: read entities and relationships as they were at this instant |
| `--fields` | Field paths to keep; see above |
| `--top`, `--next-marker` | Page size (default 50, at most 500) and the result's `nextMarker` for the next page |

## Results and limits

- `total` is how many entities the selection matched; `nextMarker` is set while more remain. Entities that change
  between calls can move between pages.
- Up to 5,000 entities, relationships and signal definitions are listed; `truncated` says when a model has more. Up to 8 Azure requests
  run at a time.
- History, signal history and annotations are the API's first page; their `nextMarker` says when more exist.
- A failed read for one entity is named in its `errors`; authentication and authorization failures fail the command.
- History, signal history and annotations use read-only POST operations underneath.


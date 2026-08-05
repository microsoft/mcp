import assert from "node:assert/strict";
import test from "node:test";
import {
  aggregateReports,
  buildReport,
  getEffectivenessCategory,
  renderMarkdown,
  type StimulusSummary,
  type TrialSnapshot,
} from "./report.js";

function trial(
  variant: string,
  stimulus: string,
  passed: boolean,
  overrides: Partial<TrialSnapshot> = {},
): TrialSnapshot {
  return {
    evalName: "eventhubs-eventhub-get-eval",
    variant,
    stimulus,
    trialIndex: 0,
    passed,
    status: "success",
    totalTokens: variant === "baseline" ? 200 : 100,
    turnCount: variant === "baseline" ? 4 : 2,
    wallTimeMs: variant === "baseline" ? 20_000 : 10_000,
    aiCredits: variant === "baseline" ? 2 : 1,
    failedGraders: [],
    ...overrides,
  };
}

function stimulus(passed: boolean): StimulusSummary {
  return {
    stimulus: "list-event-hubs",
    passed,
    trialCount: 1,
    passCount: passed ? 1 : 0,
    passRate: passed ? 1 : 0,
    avgTokens: 1,
    avgTurnCount: 1,
    avgWallTimeMs: 1,
    avgAiCredits: 1,
    failedGraders: [],
  };
}

test("classifies candidate effectiveness against baseline", () => {
  assert.equal(getEffectivenessCategory(stimulus(true), stimulus(false)), "VALUABLE");
  assert.equal(getEffectivenessCategory(stimulus(false), stimulus(true)), "REGRESSION");
  assert.equal(
    getEffectivenessCategory(stimulus(true), stimulus(true)),
    "CANDIDATE AND BASELINE PASS",
  );
  assert.equal(getEffectivenessCategory(stimulus(false), stimulus(false)), "INCONCLUSIVE");
  assert.equal(getEffectivenessCategory(stimulus(true), undefined), "NO BASELINE");
  assert.equal(getEffectivenessCategory(undefined, stimulus(true)), "NO DATA");
});

test("builds dashboard-ready report and selects best variant", () => {
  const report = buildReport([
    trial("baseline", "list-event-hubs", false),
    trial("namespace", "list-event-hubs", true),
    trial("consolidated", "list-event-hubs", true, {
      totalTokens: 150,
      turnCount: 3,
      wallTimeMs: 15_000,
      aiCredits: 1.5,
    }),
  ], "completed", "2026-08-04T00:00:00.000Z");

  assert.equal(report.schemaVersion, 2);
  assert.equal(report.failed, false);
  assert.equal(report.evals[0]?.bestVariant, "namespace");
  assert.deepEqual(
    report.evals[0]?.comparisons.map((comparison) => comparison.category),
    ["VALUABLE", "VALUABLE"],
  );
  assert.match(renderMarkdown(report), /Best variant: \*\*namespace\*\*/);
});

test("selects baseline as the best variant when it passes more efficiently", () => {
  const report = buildReport([
    trial("baseline", "update-consumer-group", true, {
      totalTokens: 24_719,
      turnCount: 2,
      wallTimeMs: 19_069,
      aiCredits: 4.22,
    }),
    trial("consolidated", "update-consumer-group", true, {
      totalTokens: 64_640,
      turnCount: 4,
      wallTimeMs: 28_682,
      aiCredits: 10.29,
    }),
    trial("namespace", "update-consumer-group", true, {
      totalTokens: 242_224,
      turnCount: 15.5,
      wallTimeMs: 86_675,
      aiCredits: 22.33,
    }),
  ]);

  assert.equal(report.evals[0]?.bestVariant, "baseline");
  assert.deepEqual(
    report.evals[0]?.comparisons.map((comparison) => comparison.category),
    ["CANDIDATE AND BASELINE PASS", "CANDIDATE AND BASELINE PASS"],
  );
  const markdown = renderMarkdown(report);
  assert.match(markdown, /Best variant: \*\*baseline\*\*/);
  assert.match(markdown, /Candidate vs baseline/);
});

test("fails report when any candidate trial fails or regresses", () => {
  const report = buildReport([
    trial("baseline", "list-event-hubs", true),
    trial("namespace", "list-event-hubs", false, {
      failedGraders: ["prompt"],
      failureEvidence: "No live Azure data returned.",
    }),
  ]);

  assert.equal(report.failed, true);
  assert.equal(report.evals[0]?.comparisons[0]?.category, "REGRESSION");
  assert.equal(
    report.evals[0]?.variants.find((variant) => variant.variant === "namespace")?.stimuli[0]?.failureEvidence,
    "No live Azure data returned.",
  );
});

test("aggregates repeated trials before selecting a variant", () => {
  const report = buildReport([
    trial("baseline", "list-event-hubs", false),
    trial("baseline", "list-event-hubs", false, { trialIndex: 1 }),
    trial("namespace", "list-event-hubs", true),
    trial("namespace", "list-event-hubs", true, { trialIndex: 1, totalTokens: 300 }),
  ]);
  const namespace = report.evals[0]?.variants.find((variant) => variant.variant === "namespace");

  assert.equal(namespace?.trialCount, 2);
  assert.equal(namespace?.avgTokens, 200);
  assert.equal(namespace?.stimuli[0]?.passed, true);
});

test("marks interrupted runs as failed", () => {
  const report = buildReport([trial("namespace", "list-event-hubs", true)], "cancelled");
  assert.equal(report.failed, true);
});

test("aggregates reports across wrapper iterations", () => {
  const first = buildReport([
    trial("baseline", "list-event-hubs", false),
    trial("namespace", "list-event-hubs", true),
  ], "completed", "2026-08-04T00:00:00.000Z");
  const second = buildReport([
    trial("baseline", "list-event-hubs", false),
    trial("namespace", "list-event-hubs", true, { totalTokens: 300 }),
  ], "completed", "2026-08-04T00:01:00.000Z");

  const aggregate = aggregateReports([first, second], "2026-08-04T00:02:00.000Z");
  const namespace = aggregate.evals[0]?.variants.find((variant) => variant.variant === "namespace");

  assert.equal(aggregate.runCount, 2);
  assert.equal(aggregate.failed, false);
  assert.equal(aggregate.evals[0]?.bestVariant, "namespace");
  assert.equal(namespace?.avgTokens, 200);
  assert.deepEqual(aggregate.evals[0]?.comparisons[0]?.categories, { VALUABLE: 2 });
});

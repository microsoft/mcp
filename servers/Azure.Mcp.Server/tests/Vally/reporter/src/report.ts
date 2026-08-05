import type {
  RunArtifacts,
  RunCompletionStatus,
  TrialResult,
  TrialWorkItem,
} from "@microsoft/vally";
import path from "node:path";

export const REPORT_FILE_NAME = "azure-mcp-report.json";
export const REPORT_MARKDOWN_FILE_NAME = "azure-mcp-report.md";
export const REPORT_SCHEMA_VERSION = 2;

const BASELINE_VARIANT = "baseline";
const NANO_AIU_PER_CREDIT = 1_000_000_000;

export type EffectivenessCategory =
  | "VALUABLE"
  | "REGRESSION"
  | "CANDIDATE AND BASELINE PASS"
  | "INCONCLUSIVE"
  | "NO BASELINE"
  | "NO DATA";

export interface TrialSnapshot {
  evalName: string;
  variant: string;
  stimulus: string;
  trialIndex: number;
  passed: boolean;
  status: TrialResult["status"];
  totalTokens: number;
  turnCount: number;
  wallTimeMs: number;
  aiCredits: number;
  failedGraders: string[];
  failureEvidence?: string;
  error?: string;
}

export interface MetricSummary {
  trialCount: number;
  passCount: number;
  passRate: number;
  avgTokens: number;
  avgTurnCount: number;
  avgWallTimeMs: number;
  avgAiCredits: number;
}

export interface StimulusSummary extends MetricSummary {
  stimulus: string;
  passed: boolean;
  failedGraders: string[];
  failureEvidence?: string;
}

export interface VariantSummary extends MetricSummary {
  variant: string;
  stimuli: StimulusSummary[];
}

export interface CandidateComparison {
  candidate: string;
  stimulus: string;
  category: EffectivenessCategory;
  candidateResult?: StimulusSummary;
  baselineResult?: StimulusSummary;
}

export interface EvalReport {
  evalName: string;
  bestVariant?: string;
  variants: VariantSummary[];
  comparisons: CandidateComparison[];
}

export interface AzureMcpVallyReport {
  schemaVersion: typeof REPORT_SCHEMA_VERSION;
  generatedAt: string;
  status: RunCompletionStatus;
  failed: boolean;
  evals: EvalReport[];
}

export interface AggregateComparison {
  candidate: string;
  stimulus: string;
  categories: Partial<Record<EffectivenessCategory, number>>;
}

export interface AggregateEvalReport {
  evalName: string;
  bestVariant?: string;
  variants: VariantSummary[];
  comparisons: AggregateComparison[];
}

export interface AzureMcpVallyAggregateReport {
  schemaVersion: typeof REPORT_SCHEMA_VERSION;
  generatedAt: string;
  failed: boolean;
  runCount: number;
  evals: AggregateEvalReport[];
  runs: AzureMcpVallyReport[];
}

export function snapshotTrial(item: TrialWorkItem, result: TrialResult): TrialSnapshot {
  const metrics = result.trajectory?.metrics;
  const failedGraders = result.grade?.details
    ?.filter((detail) => !detail.passed)
    .map((detail) => detail.name) ?? [];
  const failureEvidence = result.grade?.details?.find((detail) => !detail.passed)?.evidence
    ?? (!result.grade?.passed ? result.grade?.evidence : undefined);
  const cost = metrics?.tokenUsage.cost;

  return {
    evalName: item.evalName,
    variant: item.variant,
    stimulus: item.stimulus.name,
    trialIndex: item.trialIndex ?? 0,
    passed: result.status === "success" && (result.grade?.passed ?? false),
    status: result.status,
    totalTokens: metrics?.tokenUsage.totalTokens ?? 0,
    turnCount: metrics?.turnCount ?? 0,
    wallTimeMs: metrics?.wallTimeMs ?? result.durationMs,
    aiCredits: cost?.unit === "nano-aiu" ? cost.amount / NANO_AIU_PER_CREDIT : 0,
    failedGraders,
    ...(failureEvidence ? { failureEvidence } : {}),
    ...(result.error ? { error: result.error } : {}),
  };
}

function summarizeMetrics(trials: readonly TrialSnapshot[]): MetricSummary {
  const trialCount = trials.length;
  const passCount = trials.filter((trial) => trial.passed).length;

  return {
    trialCount,
    passCount,
    passRate: trialCount === 0 ? 0 : passCount / trialCount,
    avgTokens: average(trials.map((trial) => trial.totalTokens)),
    avgTurnCount: average(trials.map((trial) => trial.turnCount)),
    avgWallTimeMs: average(trials.map((trial) => trial.wallTimeMs)),
    avgAiCredits: average(trials.map((trial) => trial.aiCredits)),
  };
}

function summarizeStimulus(stimulus: string, trials: readonly TrialSnapshot[]): StimulusSummary {
  const failedTrial = trials.find((trial) => !trial.passed);
  return {
    stimulus,
    passed: trials.length > 0 && trials.every((trial) => trial.passed),
    failedGraders: [...new Set(trials.flatMap((trial) => trial.failedGraders))],
    ...summarizeMetrics(trials),
    ...(failedTrial?.failureEvidence ? { failureEvidence: failedTrial.failureEvidence } : {}),
  };
}

function summarizeVariant(variant: string, trials: readonly TrialSnapshot[]): VariantSummary {
  const trialsByStimulus = groupBy(trials, (trial) => trial.stimulus);
  return {
    variant,
    stimuli: [...trialsByStimulus.entries()]
      .sort(([left], [right]) => left.localeCompare(right))
      .map(([stimulus, stimulusTrials]) => summarizeStimulus(stimulus, stimulusTrials)),
    ...summarizeMetrics(trials),
  };
}

export function getEffectivenessCategory(
  candidate: StimulusSummary | undefined,
  baseline: StimulusSummary | undefined,
): EffectivenessCategory {
  if (!candidate) {
    return "NO DATA";
  }
  if (!baseline) {
    return "NO BASELINE";
  }
  if (candidate.passed && !baseline.passed) {
    return "VALUABLE";
  }
  if (!candidate.passed && baseline.passed) {
    return "REGRESSION";
  }
  if (!candidate.passed && !baseline.passed) {
    return "INCONCLUSIVE";
  }
  return "CANDIDATE AND BASELINE PASS";
}

function selectBestVariant(variants: readonly VariantSummary[]): VariantSummary | undefined {
  return [...variants].sort(compareVariantMetrics)[0];
}

function compareVariantMetrics(left: VariantSummary, right: VariantSummary): number {
  return right.passRate - left.passRate
    || left.avgTokens - right.avgTokens
    || left.avgTurnCount - right.avgTurnCount
    || left.avgWallTimeMs - right.avgWallTimeMs
    || left.avgAiCredits - right.avgAiCredits
    || left.variant.localeCompare(right.variant);
}

export function buildReport(
  trials: readonly TrialSnapshot[],
  status: RunCompletionStatus = "completed",
  generatedAt = new Date().toISOString(),
): AzureMcpVallyReport {
  const trialsByEval = groupBy(trials, (trial) => trial.evalName);
  const evals = [...trialsByEval.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([evalName, evalTrials]): EvalReport => {
      const variants = [...groupBy(evalTrials, (trial) => trial.variant).entries()]
        .map(([variant, variantTrials]) => summarizeVariant(variant, variantTrials))
        .sort((left, right) => left.variant.localeCompare(right.variant));
      const baseline = variants.find((variant) => variant.variant === BASELINE_VARIANT);
      const candidateVariants = variants.filter((variant) => variant.variant !== BASELINE_VARIANT);
      const stimulusNames = new Set(variants.flatMap((variant) => variant.stimuli.map((stimulus) => stimulus.stimulus)));
      const comparisons = candidateVariants.flatMap((candidate) =>
        [...stimulusNames].sort().map((stimulus): CandidateComparison => {
          const candidateResult = candidate.stimuli.find((result) => result.stimulus === stimulus);
          const baselineResult = baseline?.stimuli.find((result) => result.stimulus === stimulus);
          return {
            candidate: candidate.variant,
            stimulus,
            category: getEffectivenessCategory(candidateResult, baselineResult),
            ...(candidateResult ? { candidateResult } : {}),
            ...(baselineResult ? { baselineResult } : {}),
          };
        }));
      const bestVariant = selectBestVariant(variants);

      return {
        evalName,
        ...(bestVariant ? { bestVariant: bestVariant.variant } : {}),
        variants,
        comparisons,
      };
    });

  const failed = status !== "completed"
    || evals.some((evalReport) =>
      evalReport.variants
        .filter((variant) => variant.variant !== BASELINE_VARIANT)
        .some((variant) => variant.passRate < 1)
      || evalReport.comparisons.some((comparison) => comparison.category === "REGRESSION"));

  return {
    schemaVersion: REPORT_SCHEMA_VERSION,
    generatedAt,
    status,
    failed,
    evals,
  };
}

export function renderMarkdown(report: AzureMcpVallyReport): string {
  const lines = [
    "# Azure MCP Vally experiment report",
    "",
    `Status: **${report.status.toUpperCase()}**`,
    "",
  ];

  for (const evalReport of report.evals) {
    lines.push(`## ${evalReport.evalName}`, "");
    if (evalReport.bestVariant) {
      lines.push(`Best variant: **${evalReport.bestVariant}**`, "");
    }
    lines.push("| Variant | Passed | Avg tokens | Avg turns | Avg wall | Avg credits |");
    lines.push("|:--|--:|--:|--:|--:|--:|");
    for (const variant of evalReport.variants) {
      lines.push(
        `| ${variant.variant} | ${variant.passCount}/${variant.trialCount} (${formatPercent(variant.passRate)})`
        + ` | ${formatNumber(variant.avgTokens)} | ${variant.avgTurnCount.toFixed(1)}`
        + ` | ${formatSeconds(variant.avgWallTimeMs)} | ${variant.avgAiCredits.toFixed(2)} |`,
      );
    }
    lines.push("", "| Candidate | Stimulus | Candidate vs baseline |", "|:--|:--|:--|");
    for (const comparison of evalReport.comparisons) {
      lines.push(`| ${comparison.candidate} | ${comparison.stimulus} | **${comparison.category}** |`);
    }
    lines.push("");
  }

  return `${lines.join("\n")}\n`;
}

export function aggregateReports(
  reports: readonly AzureMcpVallyReport[],
  generatedAt = new Date().toISOString(),
): AzureMcpVallyAggregateReport {
  const evalsByName = groupBy(reports.flatMap((report) => report.evals), (evalReport) => evalReport.evalName);
  const evals = [...evalsByName.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([evalName, evalReports]): AggregateEvalReport => {
      const variants = [...groupBy(evalReports.flatMap((report) => report.variants), (variant) => variant.variant).entries()]
        .map(([variant, summaries]) => combineVariantSummaries(variant, summaries))
        .sort((left, right) => left.variant.localeCompare(right.variant));
      const comparisons = [...groupBy(
        evalReports.flatMap((report) => report.comparisons),
        (comparison) => `${comparison.candidate}\0${comparison.stimulus}`,
      ).values()].map((matchingComparisons): AggregateComparison => {
        const first = matchingComparisons[0];
        if (!first) {
          throw new Error("Cannot aggregate an empty comparison group.");
        }
        const categories: Partial<Record<EffectivenessCategory, number>> = {};
        for (const comparison of matchingComparisons) {
          categories[comparison.category] = (categories[comparison.category] ?? 0) + 1;
        }
        return {
          candidate: first.candidate,
          stimulus: first.stimulus,
          categories,
        };
      }).sort((left, right) =>
        left.candidate.localeCompare(right.candidate) || left.stimulus.localeCompare(right.stimulus));
      const bestVariant = selectBestVariant(variants);

      return {
        evalName,
        ...(bestVariant ? { bestVariant: bestVariant.variant } : {}),
        variants,
        comparisons,
      };
    });

  return {
    schemaVersion: REPORT_SCHEMA_VERSION,
    generatedAt,
    failed: reports.length === 0 || reports.some((report) => report.failed),
    runCount: reports.length,
    evals,
    runs: [...reports],
  };
}

export function renderAggregateMarkdown(report: AzureMcpVallyAggregateReport): string {
  const lines = [
    "# Azure MCP Vally consolidated report",
    "",
    `Runs: **${report.runCount}**`,
    "",
  ];

  for (const evalReport of report.evals) {
    lines.push(`## ${evalReport.evalName}`, "");
    if (evalReport.bestVariant) {
      lines.push(`Best variant: **${evalReport.bestVariant}**`, "");
    }
    lines.push("| Variant | Passed | Avg tokens | Avg turns | Avg wall | Avg credits |");
    lines.push("|:--|--:|--:|--:|--:|--:|");
    for (const variant of evalReport.variants) {
      lines.push(
        `| ${variant.variant} | ${variant.passCount}/${variant.trialCount} (${formatPercent(variant.passRate)})`
        + ` | ${formatNumber(variant.avgTokens)} | ${variant.avgTurnCount.toFixed(1)}`
        + ` | ${formatSeconds(variant.avgWallTimeMs)} | ${variant.avgAiCredits.toFixed(2)} |`,
      );
    }
    lines.push("", "| Candidate | Stimulus | Results across runs |", "|:--|:--|:--|");
    for (const comparison of evalReport.comparisons) {
      const categories = Object.entries(comparison.categories)
        .map(([category, count]) => `${category}: ${count}`)
        .join(", ");
      lines.push(`| ${comparison.candidate} | ${comparison.stimulus} | ${categories} |`);
    }
    lines.push("");
  }

  return `${lines.join("\n")}\n`;
}

export function resolveReportDirectory(artifacts: RunArtifacts): string | undefined {
  if (artifacts.markdown) {
    return path.dirname(artifacts.markdown);
  }
  if (artifacts.sessionLogsDir) {
    return artifacts.sessionLogsDir;
  }
  return undefined;
}

function groupBy<T>(values: readonly T[], keySelector: (value: T) => string): Map<string, T[]> {
  const groups = new Map<string, T[]>();
  for (const value of values) {
    const key = keySelector(value);
    const group = groups.get(key) ?? [];
    group.push(value);
    groups.set(key, group);
  }
  return groups;
}

function combineVariantSummaries(
  variant: string,
  summaries: readonly VariantSummary[],
): VariantSummary {
  const trialCount = summaries.reduce((sum, summary) => sum + summary.trialCount, 0);
  const passCount = summaries.reduce((sum, summary) => sum + summary.passCount, 0);
  const weightedAverage = (selector: (summary: VariantSummary) => number): number =>
    trialCount === 0
      ? 0
      : summaries.reduce((sum, summary) => sum + selector(summary) * summary.trialCount, 0) / trialCount;
  const stimuli = [...groupBy(summaries.flatMap((summary) => summary.stimuli), (stimulus) => stimulus.stimulus).entries()]
    .map(([stimulus, stimulusSummaries]) => combineStimulusSummaries(stimulus, stimulusSummaries))
    .sort((left, right) => left.stimulus.localeCompare(right.stimulus));

  return {
    variant,
    trialCount,
    passCount,
    passRate: trialCount === 0 ? 0 : passCount / trialCount,
    avgTokens: weightedAverage((summary) => summary.avgTokens),
    avgTurnCount: weightedAverage((summary) => summary.avgTurnCount),
    avgWallTimeMs: weightedAverage((summary) => summary.avgWallTimeMs),
    avgAiCredits: weightedAverage((summary) => summary.avgAiCredits),
    stimuli,
  };
}

function combineStimulusSummaries(
  stimulus: string,
  summaries: readonly StimulusSummary[],
): StimulusSummary {
  const trialCount = summaries.reduce((sum, summary) => sum + summary.trialCount, 0);
  const passCount = summaries.reduce((sum, summary) => sum + summary.passCount, 0);
  const weightedAverage = (selector: (summary: StimulusSummary) => number): number =>
    trialCount === 0
      ? 0
      : summaries.reduce((sum, summary) => sum + selector(summary) * summary.trialCount, 0) / trialCount;
  const failureEvidence = summaries.find((summary) => summary.failureEvidence)?.failureEvidence;

  return {
    stimulus,
    trialCount,
    passCount,
    passRate: trialCount === 0 ? 0 : passCount / trialCount,
    passed: trialCount > 0 && passCount === trialCount,
    avgTokens: weightedAverage((summary) => summary.avgTokens),
    avgTurnCount: weightedAverage((summary) => summary.avgTurnCount),
    avgWallTimeMs: weightedAverage((summary) => summary.avgWallTimeMs),
    avgAiCredits: weightedAverage((summary) => summary.avgAiCredits),
    failedGraders: [...new Set(summaries.flatMap((summary) => summary.failedGraders))],
    ...(failureEvidence ? { failureEvidence } : {}),
  };
}

function average(values: readonly number[]): number {
  return values.length === 0 ? 0 : values.reduce((sum, value) => sum + value, 0) / values.length;
}

function formatPercent(value: number): string {
  return `${Math.round(value * 100)}%`;
}

function formatNumber(value: number): string {
  return Math.round(value).toLocaleString("en-US");
}

function formatSeconds(milliseconds: number): string {
  return `${(milliseconds / 1000).toFixed(1)}s`;
}

export function isAzureMcpVallyReport(value: unknown): value is AzureMcpVallyReport {
  if (!value || typeof value !== "object") {
    return false;
  }
  const candidate = value as Partial<AzureMcpVallyReport>;
  return candidate.schemaVersion === REPORT_SCHEMA_VERSION
    && typeof candidate.generatedAt === "string"
    && typeof candidate.status === "string"
    && typeof candidate.failed === "boolean"
    && Array.isArray(candidate.evals);
}

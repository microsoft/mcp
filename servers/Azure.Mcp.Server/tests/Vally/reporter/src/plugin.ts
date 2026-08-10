import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";
import type {
  PlanReporter,
  ReporterRegistry,
  RunArtifacts,
  RunCompletionStatus,
  RunSummary,
  TrialResult,
  TrialWorkItem,
} from "@microsoft/vally";
import {
  buildReport,
  renderMarkdown,
  REPORT_FILE_NAME,
  REPORT_MARKDOWN_FILE_NAME,
  resolveReportDirectory,
  snapshotTrial,
  type TrialSnapshot,
} from "./report.js";

export function registerReporters(registry: ReporterRegistry): void {
  registry.register(new AzureMcpExperimentReporter());
}

export class AzureMcpExperimentReporter implements PlanReporter {
  readonly name = "azure-mcp-experiment-reporter";
  readonly #trials: TrialSnapshot[] = [];

  async onTrialResult(event: { item: TrialWorkItem; result: TrialResult }): Promise<void> {
    this.#trials.push(snapshotTrial(event.item, event.result));
  }

  async onRunComplete(
    _summary: RunSummary,
    artifacts: RunArtifacts,
    status: RunCompletionStatus = "completed",
  ): Promise<void> {
    const report = buildReport(this.#trials, status);
    const markdown = renderMarkdown(report);
    const reportDirectory = resolveReportDirectory(artifacts);

    if (!reportDirectory) {
      throw new Error("Vally did not provide an output directory for the Azure MCP experiment report.");
    }

    await mkdir(reportDirectory, { recursive: true });
    await Promise.all([
      writeFile(path.join(reportDirectory, REPORT_FILE_NAME), `${JSON.stringify(report, null, 2)}\n`, "utf8"),
      writeFile(path.join(reportDirectory, REPORT_MARKDOWN_FILE_NAME), markdown, "utf8"),
    ]);

    process.stdout.write(`\n${markdown}`);
  }
}

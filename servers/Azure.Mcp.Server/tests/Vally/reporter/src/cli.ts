#!/usr/bin/env node

import { readFile, writeFile, mkdir } from "node:fs/promises";
import path from "node:path";
import {
  aggregateReports,
  isAzureMcpVallyReport,
  renderAggregateMarkdown,
  type AzureMcpVallyReport,
} from "./report.js";

const SUMMARY_FILE_NAME = "azure-mcp-summary.json";
const SUMMARY_MARKDOWN_FILE_NAME = "azure-mcp-summary.md";

interface CliOptions {
  reports: string[];
  outputDir: string;
}

async function main(args: readonly string[]): Promise<number> {
  const options = parseArgs(args);
  const reports = await Promise.all(options.reports.map(readReport));
  const aggregate = aggregateReports(reports);
  const markdown = renderAggregateMarkdown(aggregate);

  await mkdir(options.outputDir, { recursive: true });
  await Promise.all([
    writeFile(
      path.join(options.outputDir, SUMMARY_FILE_NAME),
      `${JSON.stringify(aggregate, null, 2)}\n`,
      "utf8",
    ),
    writeFile(path.join(options.outputDir, SUMMARY_MARKDOWN_FILE_NAME), markdown, "utf8"),
  ]);
  process.stdout.write(`\n${markdown}`);
  return aggregate.failed ? 1 : 0;
}

function parseArgs(args: readonly string[]): CliOptions {
  const reports: string[] = [];
  let outputDir: string | undefined;

  for (let index = 0; index < args.length; index++) {
    const argument = args[index];
    const value = args[index + 1];
    if (argument === "--report" && value) {
      reports.push(path.resolve(value));
      index++;
    } else if (argument === "--output-dir" && value) {
      outputDir = path.resolve(value);
      index++;
    } else {
      throw new Error(`Unknown or incomplete argument: ${argument ?? "<missing>"}`);
    }
  }

  if (reports.length === 0) {
    throw new Error("At least one --report <path> argument is required.");
  }
  if (!outputDir) {
    throw new Error("--output-dir <path> is required.");
  }

  return { reports, outputDir };
}

async function readReport(reportPath: string): Promise<AzureMcpVallyReport> {
  const parsed: unknown = JSON.parse(await readFile(reportPath, "utf8"));
  if (!isAzureMcpVallyReport(parsed)) {
    throw new Error(`Unsupported or malformed Azure MCP Vally report: ${reportPath}`);
  }
  return parsed;
}

main(process.argv.slice(2))
  .then((exitCode) => {
    process.exitCode = exitCode;
  })
  .catch((error: unknown) => {
    const message = error instanceof Error ? error.message : String(error);
    process.stderr.write(`[azure-mcp-vally-report] ${message}\n`);
    process.exitCode = 2;
  });

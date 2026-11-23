// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Text;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// Adds an 'At a glance' section of charts to the ROI report (ROI.md) the HA skill produces: headline figures, a pie of the current
/// annual cost split into future cost and savings, savings by year and cost by pillar. The figures are read from the report's own
/// tables; nothing is derived except chart proportions. The section is HTML with inline styles only, because Markdown previews
/// such as the Cowork file preview drop &lt;style&gt; blocks and inline SVG but keep inline styles, tables and CSS gradients. It sits
/// between <see cref="StartMarker"/> and <see cref="EndMarker"/> with no blank lines inside, so Markdown keeps it as one HTML block.
/// A report whose tables cannot be read is returned unchanged.
/// </summary>
internal static class RoiVisuals
{
    internal const string StartMarker = "<!-- azure-catalyst:visuals -->";
    internal const string EndMarker = "<!-- /azure-catalyst:visuals -->";

    private const string Blue = "#0078d4";
    private const string Green = "#107c10";
    private const string Grey = "#8a8886";
    private const string Track = "#edebe9";
    private const string Border = "#d1d9e0";
    private const string Muted = "#605e5c";

    /// <summary>Returns the report with the charts section added after its title, or unchanged when the figures are not found.</summary>
    internal static string Enrich(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown) || markdown.Contains(StartMarker, StringComparison.Ordinal))
        {
            return markdown;
        }

        string? html;
        try
        {
            html = Build(ReadTables(markdown));
        }
        catch (FormatException)
        {
            html = null;
        }

        if (html is null)
        {
            return markdown;
        }

        string text = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        int at = text.IndexOf("\n## ", StringComparison.Ordinal);
        // Blank lines around the block: Markdown ends an HTML block only at a blank line.
        string block = "\n" + html + "\n\n";
        return at < 0 ? text.TrimEnd() + "\n" + block : text[..(at + 1)] + block + text[(at + 1)..];
    }

    private static string? Build(List<MdTable> tables)
    {
        // Full-cost estimate: the basis of the report's headline ROI, NPV and payback, strategies ordered by savings.
        MdTable? annual = tables.FirstOrDefault(t => t.H2 == "Annual comparison" && t.Has("Current on-premises (annual)") && t.Has("Future (annual)") && t.Has("Savings (annual)"))
            ?? tables.FirstOrDefault(t => t.H2 == "Annual comparison" && t.Has("Current on-premises (annual)") && t.Has("Azure (annual)") && t.Has("Savings (annual)"));
        if (annual is null || annual.Rows.Count == 0)
        {
            return null;
        }

        string futureColumn = annual.Has("Future (annual)") ? "Future (annual)" : "Azure (annual)";
        List<string[]> rows = annual.Rows.Where(r => Number(annual.Cell(r, "Current on-premises (annual)")) is not null).ToList();
        if (rows.Count == 0)
        {
            return null;
        }

        string[] best = rows.OrderByDescending(r => Number(annual.Cell(r, "Savings (annual)")) ?? double.MinValue).First();
        string strategy = annual.Cell(best, "Strategy");
        double current = Number(annual.Cell(best, "Current on-premises (annual)"))!.Value;
        double future = Number(annual.Cell(best, futureColumn)) ?? 0;
        double savings = Number(annual.Cell(best, "Savings (annual)")) ?? current - future;
        string currency = annual.Cell(best, "Currency");
        string basis = annual.Has("Future (annual)") ? "Full-cost estimate" : "Azure Migrate method estimate";

        MdTable? finance = tables.FirstOrDefault(t => t.Has("ROI") && t.Has("NPV") && t.Rows.Any(r => t.Cell(r, "Strategy") == strategy)
            && (t.Cell(t.Rows.First(r => t.Cell(r, "Strategy") == strategy), "Basis").StartsWith(basis, StringComparison.Ordinal) || !t.Has("Basis")));
        string[]? financeRow = finance?.Rows.First(r => finance.Cell(r, "Strategy") == strategy);

        var h = new StringBuilder();
        h.Append(StartMarker).Append('\n');
        h.Append("<div style=\"border:1px solid ").Append(Border).Append(";border-radius:8px;padding:12px 16px;margin:8px 0 16px\">\n");
        h.Append("<div style=\"font-size:16px;font-weight:600;margin-bottom:2px\">At a glance: ").Append(E(strategy)).Append("</div>\n");
        h.Append("<div style=\"font-size:12px;color:").Append(Muted).Append(";margin-bottom:10px\">").Append(E(basis))
            .Append(currency.Length > 0 ? ", " + E(currency) : string.Empty).Append(", annual figures from the tables below.</div>\n");

        // Headline figures.
        var tiles = new List<(string Label, string Value, string Color)>
        {
            ("Annual savings", Money(annual.Cell(best, "Savings (annual)"), currency), savings >= 0 ? Green : "#a4262c"),
        };
        if (financeRow is not null)
        {
            AddTile(tiles, "ROI", finance!.Cell(financeRow, "ROI"), Blue);
            AddTile(tiles, "NPV", Money(finance.Cell(financeRow, "NPV"), currency), Blue);
            string months = finance.Cell(financeRow, "Payback (months)");
            AddTile(tiles, "Payback", Number(months) is double m && m >= 0 ? months + " months" : finance.Cell(financeRow, "Payback (years)") is { Length: > 0 } y ? y + " years" : string.Empty, Blue);
        }

        h.Append("<table style=\"border-collapse:separate;border-spacing:8px 0;margin:0 -8px 12px\"><tr>");
        foreach ((string label, string value, string color) in tiles)
        {
            h.Append("<td style=\"border:1px solid ").Append(Border).Append(";border-radius:6px;padding:8px 14px;min-width:110px;vertical-align:top\">")
                .Append("<div style=\"font-size:11px;color:").Append(Muted).Append("\">").Append(E(label)).Append("</div>")
                .Append("<div style=\"font-size:20px;font-weight:600;color:").Append(color).Append("\">").Append(E(value)).Append("</div></td>");
        }

        h.Append("</tr></table>\n");

        // Pie: the current annual cost split into what is still spent (future cost) and what is saved.
        if (current > 0 && savings > 0 && future >= 0)
        {
            double share = Math.Clamp(savings / current * 100, 0, 100);
            string s = share.ToString("0.#", CultureInfo.InvariantCulture);
            h.Append("<table style=\"border-collapse:collapse;margin-bottom:12px\"><tr>")
                .Append("<td style=\"padding:0 16px 0 0;vertical-align:middle\"><div style=\"width:120px;height:120px;border-radius:50%;background:conic-gradient(")
                .Append(Green).Append(" 0 ").Append(s).Append("%,").Append(Blue).Append(' ').Append(s).Append("% 100%)\"></div></td>")
                .Append("<td style=\"vertical-align:middle;font-size:13px\">")
                .Append("<div style=\"font-weight:600;margin-bottom:6px\">Current annual cost: ").Append(E(Money(annual.Cell(best, "Current on-premises (annual)"), currency))).Append("</div>")
                .Append(Legend(Green, "Saved", Money(annual.Cell(best, "Savings (annual)"), currency) + " (" + s + "%)"))
                .Append(Legend(Blue, futureColumn == "Future (annual)" ? "Future cost" : "Azure cost", Money(annual.Cell(best, futureColumn), currency) + " (" + (100 - share).ToString("0.#", CultureInfo.InvariantCulture) + "%)"))
                .Append("</td></tr></table>\n");
        }

        // Savings by strategy, when there is more than one.
        if (rows.Count > 1)
        {
            double max = rows.Max(r => Math.Abs(Number(annual.Cell(r, "Savings (annual)")) ?? 0));
            h.Append(Title("Annual savings by strategy")).Append(BarsOpen());
            foreach (string[] r in rows)
            {
                double v = Number(annual.Cell(r, "Savings (annual)")) ?? 0;
                h.Append(BarRow(annual.Cell(r, "Strategy"), [(v, v >= 0 ? Green : "#a4262c")], max, Money(annual.Cell(r, "Savings (annual)"), currency)));
            }

            h.Append("</table>\n");
        }

        // Savings by year.
        MdTable? yearly = tables.FirstOrDefault(t => t.H2 == "Yearly view" && t.H3.StartsWith(strategy + " (" + basis, StringComparison.Ordinal) && t.Has("Savings"));
        if (yearly is not null && yearly.Rows.Count > 0)
        {
            double max = yearly.Rows.Max(r => Math.Abs(Number(yearly.Cell(r, "Savings")) ?? 0));
            if (max > 0)
            {
                h.Append(Title("Savings by year")).Append(BarsOpen());
                foreach (string[] r in yearly.Rows)
                {
                    string year = yearly.Cell(r, "Year");
                    double v = Number(yearly.Cell(r, "Savings")) ?? 0;
                    h.Append(BarRow(year.StartsWith("Year", StringComparison.Ordinal) ? year : "Year " + year, [(v, Green)], max, Money(yearly.Cell(r, "Savings"), currency)));
                }

                h.Append("</table>\n");
            }
        }

        // Cost by pillar: current against Azure.
        MdTable? pillars = tables.FirstOrDefault(t => t.H2 == "Cost by pillar" && t.H3 == strategy && t.Has("Pillar") && t.Has("Current (annual)") && t.Has("Azure (annual)"));
        if (pillars is not null)
        {
            // A savings row (for example Extended Security Update savings) is not a cost, so it is not charted as one.
            List<string[]> pr = pillars.Rows.Where(r => !pillars.Cell(r, "Pillar").Contains("savings", StringComparison.OrdinalIgnoreCase)
                && ((Number(pillars.Cell(r, "Current (annual)")) ?? 0) > 0 || (Number(pillars.Cell(r, "Azure (annual)")) ?? 0) > 0)).ToList();
            double max = pr.Count == 0 ? 0 : pr.Max(r => Math.Max(Number(pillars.Cell(r, "Current (annual)")) ?? 0, Number(pillars.Cell(r, "Azure (annual)")) ?? 0));
            if (max > 0)
            {
                h.Append(Title("Annual cost by pillar"))
                    .Append("<div style=\"font-size:12px;margin:0 0 4px\">").Append(Legend(Grey, "Current", string.Empty, inline: true)).Append(Legend(Blue, "Azure", string.Empty, inline: true)).Append("</div>\n")
                    .Append(BarsOpen());
                foreach (string[] r in pr)
                {
                    h.Append(BarRow(pillars.Cell(r, "Pillar"),
                        [(Number(pillars.Cell(r, "Current (annual)")) ?? 0, Grey), (Number(pillars.Cell(r, "Azure (annual)")) ?? 0, Blue)], max,
                        pillars.Cell(r, "Current (annual)") + " → " + pillars.Cell(r, "Azure (annual)")));
                }

                h.Append("</table>\n");
            }
        }

        h.Append("</div>\n").Append(EndMarker);
        return h.ToString();
    }

    private static void AddTile(List<(string, string, string)> tiles, string label, string value, string color)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            tiles.Add((label, value, color));
        }
    }

    private static string Title(string text) =>
        "<div style=\"font-size:13px;font-weight:600;margin:10px 0 4px\">" + E(text) + "</div>\n";

    private static string BarsOpen() => "<table style=\"border-collapse:collapse;width:100%;font-size:12px;margin-bottom:6px\">";

    private static string BarRow(string label, (double Value, string Color)[] bars, double max, string text)
    {
        var b = new StringBuilder();
        b.Append("<tr><td style=\"padding:2px 10px 2px 0;white-space:nowrap;width:1%\">").Append(E(label)).Append("</td><td style=\"padding:2px 0;width:70%\">");
        foreach ((double value, string color) in bars)
        {
            double pct = max <= 0 ? 0 : Math.Clamp(Math.Abs(value) / max * 100, 0, 100);
            b.Append("<div style=\"background:").Append(Track).Append(";border-radius:3px;margin:1px 0\"><div style=\"background:").Append(color)
                .Append(";width:").Append(pct.ToString("0.#", CultureInfo.InvariantCulture)).Append("%;height:10px;border-radius:3px\"></div></div>");
        }

        b.Append("</td><td style=\"padding:2px 0 2px 10px;white-space:nowrap;text-align:right;font-variant-numeric:tabular-nums\">").Append(E(text)).Append("</td></tr>");
        return b.ToString();
    }

    private static string Legend(string color, string label, string value, bool inline = false) =>
        "<" + (inline ? "span" : "div") + " style=\"margin:2px 12px 2px 0" + (inline ? ";display:inline-block" : string.Empty) + "\"><span style=\"display:inline-block;width:10px;height:10px;border-radius:2px;background:" +
        color + ";margin-right:6px\"></span>" + E(label) + (value.Length > 0 ? ": " + E(value) : string.Empty) + "</" + (inline ? "span" : "div") + ">";

    private static string Money(string value, string currency) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : currency is { Length: > 0 } && Number(value) is not null ? currency + " " + value : value;

    private static string E(string text) => WebUtility.HtmlEncode(text);

    private static double? Number(string text)
    {
        string t = text.Trim().TrimEnd('%').Replace(",", string.Empty, StringComparison.Ordinal);
        return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : null;
    }

    private static List<MdTable> ReadTables(string markdown)
    {
        var tables = new List<MdTable>();
        string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        string h2 = string.Empty, h3 = string.Empty;
        bool inCode = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                h2 = line[3..].Trim();
                h3 = string.Empty;
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                h3 = line[4..].Trim();
            }
            else if (line.StartsWith('|') && i + 1 < lines.Length && IsSeparator(lines[i + 1]))
            {
                var table = new MdTable(h2, h3, Cells(line));
                i += 2;
                while (i < lines.Length && lines[i].Trim().StartsWith('|'))
                {
                    table.Rows.Add(Cells(lines[i]));
                    i++;
                }

                i--;
                tables.Add(table);
            }
        }

        return tables;
    }

    private static bool IsSeparator(string line)
    {
        string t = line.Trim();
        return t.StartsWith('|') && t.Contains("---", StringComparison.Ordinal) && t.All(c => c is '|' or '-' or ':' or ' ');
    }

    private static string[] Cells(string line)
    {
        string t = line.Trim();
        if (t.StartsWith('|'))
        {
            t = t[1..];
        }

        if (t.EndsWith('|'))
        {
            t = t[..^1];
        }

        return t.Split('|').Select(c => c.Trim()).ToArray();
    }

    private sealed class MdTable(string h2, string h3, string[] header)
    {
        public string H2 { get; } = h2;

        public string H3 { get; } = h3;

        public List<string[]> Rows { get; } = [];

        public bool Has(string column) => Array.IndexOf(header, column) >= 0;

        public string Cell(string[] row, string column)
        {
            int i = Array.IndexOf(header, column);
            return i >= 0 && i < row.Length ? row[i] : string.Empty;
        }
    }
}

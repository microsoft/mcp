// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.McpApps;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// POC: MCP Apps card for the business case create and status tools. Shown with the create result, it tracks the business case by
/// calling the status tool itself (each call moves the business case to its next stage) and updates its stage in place, so the agent
/// does not poll and the chat shows one card instead of a tool call per check. Once the business case succeeds it renders the ROI
/// report ('report', the ROI.md Markdown) like an editor's Markdown preview (headings, tables, lists, code), with buttons to open the
/// full view, copy the Markdown and download the report. It tells the agent the latest state through ui/update-model-context and
/// never posts chat messages. The report comes in the tool result, so the card needs no connect domains. Everything is escaped
/// before rendering and only http(s) links are kept.
/// </summary>
internal static class BusinessCaseReportWidget
{
    internal const string ResourceUri = "ui://azurecatalyst/business-case-report.html";

    /// <summary>Set to 'false' to turn the card off.</summary>
    internal const string EnabledSetting = "AZMIGRATE_REPORT_WIDGET_ENABLED";

    internal static bool IsEnabled =>
        !string.Equals(Environment.GetEnvironmentVariable(EnabledSetting)?.Trim(), "false", StringComparison.OrdinalIgnoreCase);

    /// <summary>Registers the card for the given command (the business case create or status command) when enabled.</summary>
    internal static void Register(string commandId)
    {
        if (IsEnabled)
        {
            McpAppRegistry.Register(commandId, new McpAppWidget(ResourceUri, "Azure Catalyst business case report", Html, []));
        }
    }

    internal const string Html = """
        <!doctype html>
        <html>
        <head><meta charset="utf-8"><title>Business case report</title>
        <style>
        :root{color-scheme:light dark;--fg:#1f2328;--muted:#59636e;--border:#d1d9e0;--bg:#fff;--alt:#f6f8fa;--code:#eff1f3;--link:#0969da;--ok:#1a7f37;--err:#d1242f}
        @media (prefers-color-scheme: dark){:root{--fg:#e6edf3;--muted:#9198a1;--border:#3d444d;--bg:#0d1117;--alt:#151b23;--code:#262c36;--link:#4493f8;--ok:#3fb950;--err:#f85149}}
        html,body{margin:0;background:var(--bg);color:var(--fg)}
        body{font:14px/1.5 -apple-system,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif}
        .bar{display:flex;align-items:center;gap:8px;padding:8px 12px;border-bottom:1px solid var(--border);position:sticky;top:0;background:var(--bg);z-index:1}
        .bar .title{font-weight:600;flex:1;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
        .bar .sub{color:var(--muted);font-weight:400;margin-left:6px}
        button{font:inherit;font-size:12px;padding:3px 10px;border:1px solid var(--border);border-radius:6px;background:var(--alt);color:var(--fg);cursor:pointer}
        button:hover{border-color:var(--muted)}
        .state{padding:10px 12px;color:var(--muted)}.state.ok{color:var(--ok)}.state.err{color:var(--err)}
        #doc{padding:4px 20px 16px;overflow:auto}
        #doc.collapsed{max-height:520px}
        body.fullscreen #doc{max-height:none}
        body.fullscreen .more{display:none !important}
        .more{display:none;padding:6px 12px;border-top:1px solid var(--border);text-align:center}
        .markdown h1,.markdown h2{padding-bottom:.3em;border-bottom:1px solid var(--border)}
        .markdown h1{font-size:1.8em;margin:.6em 0 .5em}.markdown h2{font-size:1.4em;margin:1.2em 0 .6em}
        .markdown h3{font-size:1.15em;margin:1.1em 0 .5em}.markdown h4,.markdown h5,.markdown h6{font-size:1em;margin:1em 0 .4em}
        .markdown p,.markdown ul,.markdown ol,.markdown blockquote,.markdown pre,.markdown .tw{margin:0 0 12px}
        .markdown ul,.markdown ol{padding-left:2em}.markdown li+li{margin-top:.2em}
        .markdown code{font:12px/1.4 ui-monospace,SFMono-Regular,Consolas,"Liberation Mono",monospace;background:var(--code);padding:.15em .35em;border-radius:5px}
        .markdown pre{background:var(--alt);padding:12px;border-radius:6px;overflow:auto}.markdown pre code{background:none;padding:0}
        .markdown blockquote{padding:0 1em;color:var(--muted);border-left:.25em solid var(--border)}
        .markdown hr{border:0;height:2px;background:var(--border);margin:20px 0}
        .markdown a{color:var(--link);text-decoration:none}.markdown a:hover{text-decoration:underline}
        .markdown .tw{overflow-x:auto}
        .markdown table{border-collapse:collapse;font-size:13px}
        .markdown th,.markdown td{border:1px solid var(--border);padding:5px 12px;vertical-align:top}
        .markdown th{font-weight:600;background:var(--alt)}
        .markdown tr:nth-child(2n) td{background:var(--alt)}
        .markdown td.num{text-align:right;font-variant-numeric:tabular-nums;white-space:nowrap}
        </style>
        </head>
        <body>
        <div class="bar"><span class="title" id="title">Business case<span class="sub" id="sub"></span></span>
          <button id="full" style="display:none">Open full view</button>
          <button id="copy" style="display:none">Copy Markdown</button>
          <button id="open" style="display:none">Download ROI.md</button></div>
        <div class="state" id="state">Waiting for the business case...</div>
        <div id="doc" class="markdown collapsed"></div>
        <div class="more" id="more"><button id="toggle">Show full report</button></div>
        <script>
        (function () {
          var el = function (id) { return document.getElementById(id); };
          var ctx = { report: null, link: null };
          function size() {
            var h = Math.ceil(document.body.getBoundingClientRect().height) + 4;
            window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/size-changed', params: { width: document.documentElement.scrollWidth, height: h } }, '*');
          }
          var nextId = 1, pending = {};
          function send(method, params, timeoutMs) {
            var id = nextId++;
            window.parent.postMessage({ jsonrpc: '2.0', id: id, method: method, params: params }, '*');
            return new Promise(function (resolve, reject) {
              pending[id] = { resolve: resolve, reject: reject };
              setTimeout(function () { if (pending[id]) { delete pending[id]; reject(new Error('timeout ' + method)); } }, timeoutMs || 10000);
            });
          }

          // ---- Markdown (CommonMark/GFM subset): escape first, then add only known markup. ----
          function esc(s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }
          function inline(s) {
            var codes = [];
            s = s.replace(/`([^`]+)`/g, function (_, c) { codes.push(c); return '\u0000' + (codes.length - 1) + '\u0000'; });
            s = esc(s)
              .replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g, '<a href="$2" data-link="1">$1</a>')
              .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>').replace(/__([^_]+)__/g, '<strong>$1</strong>')
              .replace(/(^|[^*])\*([^*\s][^*]*)\*/g, '$1<em>$2</em>').replace(/(^|\W)_([^_\s][^_]*)_(?=\W|$)/g, '$1<em>$2</em>');
            return s.replace(/\u0000(\d+)\u0000/g, function (_, i) { return '<code>' + esc(codes[+i]) + '</code>'; });
          }
          function cells(line) {
            var t = line.trim(); if (t.charAt(0) === '|') { t = t.slice(1); } if (t.charAt(t.length - 1) === '|') { t = t.slice(0, -1); }
            return t.split(/(?<!\\)\|/).map(function (c) { return c.trim().replace(/\\\|/g, '|'); });
          }
          var isSep = function (l) { return /^\s*\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)*\|?\s*$/.test(l); };
          var isNum = function (s) { return /^[-+]?[\d,]*\.?\d+%?$/.test(s); };
          function table(lines) {
            var head = cells(lines[0]), align = cells(lines[1]).map(function (a) {
              return /^:-+:$/.test(a) ? 'center' : /-:$/.test(a) ? 'right' : /^:-/.test(a) ? 'left' : '';
            });
            var h = '<div class="tw"><table><thead><tr>' + head.map(function (c, i) {
              return '<th' + (align[i] ? ' style="text-align:' + align[i] + '"' : '') + '>' + inline(c) + '</th>';
            }).join('') + '</tr></thead><tbody>';
            lines.slice(2).forEach(function (l) {
              h += '<tr>' + cells(l).map(function (c, i) {
                var st = align[i] ? ' style="text-align:' + align[i] + '"' : '';
                return '<td' + (isNum(c) && !align[i] ? ' class="num"' : '') + st + '>' + inline(c) + '</td>';
              }).join('') + '</tr>';
            });
            return h + '</tbody></table></div>';
          }
          function list(lines) {
            var html = '', stack = [];
            lines.forEach(function (l) {
              var m = /^(\s*)([-*+]|\d+[.)])\s+(.*)$/.exec(l);
              if (!m) { html += ' ' + inline(l.trim()); return; }
              var ind = m[1].replace(/\t/g, '    ').length, tag = /\d/.test(m[2]) ? 'ol' : 'ul';
              while (stack.length && ind < stack[stack.length - 1].ind) { html += '</li></' + stack.pop().tag + '>'; }
              if (!stack.length || ind > stack[stack.length - 1].ind) { html += '<' + tag + '>'; stack.push({ ind: ind, tag: tag }); }
              else { html += '</li>'; }
              html += '<li>' + inline(m[3]);
            });
            while (stack.length) { html += '</li></' + stack.pop().tag + '>'; }
            return html;
          }
          function markdown(text) {
            var L = String(text).replace(/\r\n?/g, '\n').split('\n'), out = '', i = 0;
            var isList = function (l) { return /^\s*([-*+]|\d+[.)])\s+/.test(l); };
            var isBlockStart = function (l, n) {
              return /^\s*(#{1,6}\s|```|>|(\*\s*){3,}$|(-\s*){3,}$)/.test(l) || isList(l) || (l.indexOf('|') >= 0 && n !== undefined && isSep(n));
            };
            while (i < L.length) {
              var l = L[i];
              if (!l.trim()) { i++; continue; }
              var m;
              if (/^\s*```/.test(l)) {
                var code = []; i++;
                while (i < L.length && !/^\s*```/.test(L[i])) { code.push(L[i++]); }
                i++; out += '<pre><code>' + esc(code.join('\n')) + '</code></pre>'; continue;
              }
              if ((m = /^\s*(#{1,6})\s+(.*?)\s*#*\s*$/.exec(l))) { out += '<h' + m[1].length + '>' + inline(m[2]) + '</h' + m[1].length + '>'; i++; continue; }
              if (/^\s*((\*\s*){3,}|(-\s*){3,}|(_\s*){3,})$/.test(l)) { out += '<hr>'; i++; continue; }
              if (l.indexOf('|') >= 0 && i + 1 < L.length && isSep(L[i + 1])) {
                var t = [l, L[i + 1]]; i += 2;
                while (i < L.length && L[i].trim() && L[i].indexOf('|') >= 0) { t.push(L[i++]); }
                out += table(t); continue;
              }
              if (/^\s*>/.test(l)) {
                var q = [];
                while (i < L.length && /^\s*>/.test(L[i])) { q.push(L[i++].replace(/^\s*>\s?/, '')); }
                out += '<blockquote>' + markdown(q.join('\n')) + '</blockquote>'; continue;
              }
              if (isList(l)) {
                var items = [];
                while (i < L.length && L[i].trim() && (isList(L[i]) || /^\s+\S/.test(L[i]) || !isBlockStart(L[i], L[i + 1]))) { items.push(L[i++]); }
                out += list(items); continue;
              }
              var p = [];
              while (i < L.length && L[i].trim() && !isBlockStart(L[i], L[i + 1])) { p.push(L[i++].trim()); }
              if (!p.length) { p.push(L[i++].trim()); }
              out += '<p>' + inline(p.join(' ')) + '</p>';
            }
            return out;
          }

          // ---- Tool result ----
          function findResult(node, depth) {
            if (depth > 8 || node === null || node === undefined) { return null; }
            if (typeof node === 'string') {
              var s = node.trim();
              if (s.charAt(0) === '{' || s.charAt(0) === '[') { try { return findResult(JSON.parse(s), depth + 1); } catch (e) { return null; } }
              return null;
            }
            if (typeof node !== 'object') { return null; }
            if ('totalStages' in node || ('success' in node && ('report' in node || 'status' in node || 'error' in node))) { return node; }
            for (var k in node) { var r = findResult(node[k], depth + 1); if (r) { return r; } }
            return null;
          }
          function state(text, cls) { var s = el('state'); s.textContent = text; s.className = 'state ' + (cls || ''); s.style.display = text ? '' : 'none'; }
          var stageText = { Assessment: 'assessing the servers and evaluating costs', HaSkill: 'building the ROI report' };
          function elapsed() {
            if (!ctx.startedAt) { return ''; }
            var m = Math.floor((Date.now() - ctx.startedAt) / 60000);
            return m < 1 ? '' : ' (' + m + ' min)';
          }
          function render(r) {
            if (r.assessmentName) { el('sub').textContent = r.assessmentName; }
            if (r.success === false) { state(r.error || 'The business case request was not valid.', 'err'); size(); return; }
            if (r.status === 'Succeeded' && r.report) {
              ctx.report = r.report; ctx.link = /^https:\/\//.test(r.result || '') ? r.result : null;
              el('title').firstChild.textContent = 'ROI report';
              state('');
              el('doc').innerHTML = markdown(r.report);
              el('copy').style.display = '';
              el('full').style.display = '';
              el('open').style.display = ctx.link ? '' : 'none';
              el('more').style.display = el('doc').scrollHeight > el('doc').clientHeight + 4 ? 'block' : 'none';
            } else if (r.status === 'Succeeded') {
              state('The business case is ready. Ask Copilot in the chat for the report link.', 'ok');
            } else if (r.status === 'Failed') {
              state('The business case failed' + (r.errorMessage ? ': ' + r.errorMessage : '.'), 'err');
            } else {
              var n = r.stageNumber || 1;
              state('Stage ' + n + ' of ' + (r.totalStages || 2) + ': ' + (stageText[r.stage] || 'running') + '...' + elapsed() +
                ' This card updates by itself.', '');
            }
            size();
          }

          // ---- Tracking ----
          // The card calls the status tool itself until the business case finishes, so the agent does not poll and the chat shows one
          // card instead of a tool call per check. Each status call also moves the business case to its next stage. Calls must stay
          // short: hosts cut off a card's tools/call after about 27 seconds. The latest state is saved per business case so a reloaded
          // card resumes from it instead of starting the business case's next stage again; the agent is told the latest state through
          // ui/update-model-context so it can answer questions about it. It never sends ui/message: hosts show it as the user's message.
          var STATUS_TOOL = 'azure_catalyst_businesscase_status', MAX_CHECKS = 120, MAX_FAILURES = 3;
          var tracking = false, checks = 0, failures = 0;
          function storageKey(r) { return r && r.asOperationId ? 'azurecatalyst-bc-' + r.asOperationId : null; }
          function load(r) {
            try { var k = storageKey(r), v = k && window.localStorage.getItem(k); return v ? JSON.parse(v) : null; } catch (e) { return null; }
          }
          function save(r) {
            try { var k = storageKey(r); if (k) { window.localStorage.setItem(k, JSON.stringify(r)); } } catch (e) { }
          }
          function clean(p) {
            var o = {};
            for (var k in p) { if (p[k] !== null && p[k] !== undefined) { o[k] = p[k]; } }
            return o;
          }
          function tellModel(r) {
            var lines = ['Business case card update for "' + (r.assessmentName || r.asOperationId) + '": status ' + r.status +
              ', stage ' + (r.stageNumber || 1) + ' of ' + (r.totalStages || 2) + '.'];
            if (r.status === 'Running' && r.nextStatusParameters) {
              lines.push('The card is tracking it. If the user asks for the status, use these nextStatusParameters: ' + JSON.stringify(clean(r.nextStatusParameters)));
            } else if (r.status === 'Succeeded') {
              lines.push('The ROI report is shown to the user in the card.' + (r.result ? ' Report link: ' + r.result : ''));
              if (r.report) { lines.push('ROI report (ROI.md):\n' + String(r.report).slice(0, 12000)); }
            } else if (r.status === 'Failed') {
              lines.push('Error: ' + (r.errorMessage || 'unknown') + '.');
            }
            send('ui/update-model-context', { content: [{ type: 'text', text: lines.join('\n') }] }, 5000).catch(function () { });
          }
          function schedule(r) {
            ctx.params = clean(r.nextStatusParameters);
            var wait = Math.min(Math.max(r.retryAfterSeconds || 30, 10), 60);
            setTimeout(check, wait * 1000);
          }
          function check() {
            if (++checks > MAX_CHECKS) {
              tracking = false;
              state('The business case is taking longer than expected. Ask Copilot in the chat for its status.', '');
              size(); return;
            }
            send('tools/call', { name: STATUS_TOOL, arguments: ctx.params }, 25000).then(function (res) {
              var s = findResult(res, 0);
              if (!s || s.success === false) { throw new Error((s && s.error) || 'unreadable status'); }
              failures = 0;
              var changed = !ctx.last || ctx.last.status !== s.status || ctx.last.stage !== s.stage;
              ctx.last = s; save(s); render(s);
              if (changed) { tellModel(s); }
              if (s.status === 'Running' && s.nextStatusParameters) { schedule(s); } else { tracking = false; }
            }).catch(function () {
              // A cut-off call may still have moved the business case on; the next call with the same parameters picks that up.
              if (++failures >= MAX_FAILURES) {
                tracking = false;
                state('Could not check the business case status here. Ask Copilot in the chat for its status.', '');
                size(); return;
              }
              setTimeout(check, 30000);
            });
          }
          function onToolResult(params) {
            var r = findResult(params, 0);
            if (!r) { size(); return; }
            var saved = load(r);
            if (saved && (saved.isCompleted || (saved.stageNumber || 1) >= (r.stageNumber || 1))) { r = saved; }
            ctx.last = r;
            if (!ctx.startedAt) { ctx.startedAt = Date.now(); }
            render(r);
            if (!tracking && r.success !== false && r.status === 'Running' && r.nextStatusParameters) { tracking = true; schedule(r); }
          }
          window.addEventListener('message', function (e) {
            var m = e.data;
            if (!m || m.jsonrpc !== '2.0') { return; }
            if (m.method) {
              if (m.method === 'ui/notifications/tool-result') { onToolResult(m.params); }
              if (m.method === 'ui/notifications/host-context-changed' && m.params) { applyHostContext(m.params); }
              if (m.id !== undefined) { window.parent.postMessage({ jsonrpc: '2.0', id: m.id, result: {} }, '*'); }
            } else if (m.id !== undefined && pending[m.id]) {
              var p = pending[m.id]; delete pending[m.id];
              if (m.error) { p.reject(new Error(JSON.stringify(m.error))); } else { p.resolve(m.result); }
            }
          });
          // Sandboxed frames cannot navigate, so links go through the host; window.open is the fallback.
          function openLink(url) { send('ui/open-link', { url: url }).catch(function () { window.open(url, '_blank', 'noopener'); }); }
          el('open').addEventListener('click', function () { if (ctx.link) { openLink(ctx.link); } });
          el('doc').addEventListener('click', function (e) {
            var a = e.target.closest && e.target.closest('a[data-link]');
            if (a) { e.preventDefault(); openLink(a.getAttribute('href')); }
          });
          el('copy').addEventListener('click', function () {
            var done = function () { el('copy').textContent = 'Copied'; setTimeout(function () { el('copy').textContent = 'Copy Markdown'; }, 1500); };
            if (navigator.clipboard && navigator.clipboard.writeText) {
              navigator.clipboard.writeText(ctx.report).then(done, fallback);
            } else { fallback(); }
            function fallback() {
              var ta = document.createElement('textarea'); ta.value = ctx.report; document.body.appendChild(ta); ta.select();
              try { document.execCommand('copy'); done(); } catch (e) { el('copy').textContent = 'Copy failed'; }
              ta.remove();
            }
          });
          el('toggle').addEventListener('click', function () {
            var d = el('doc'); d.classList.toggle('collapsed');
            el('toggle').textContent = d.classList.contains('collapsed') ? 'Show full report' : 'Collapse report';
            size();
          });
          // Full view: ask the host for its fullscreen display mode (MCP Apps); hosts without it get the full report inline.
          var displayMode = 'inline';
          function setMode(mode) {
            displayMode = mode === 'fullscreen' ? 'fullscreen' : 'inline';
            document.body.classList.toggle('fullscreen', displayMode === 'fullscreen');
            el('full').textContent = displayMode === 'fullscreen' ? 'Exit full view' : 'Open full view';
            size();
          }
          function applyHostContext(hc) {
            if (hc && hc.displayMode) { setMode(hc.displayMode); }
          }
          function expandInline() {
            el('doc').classList.remove('collapsed'); el('toggle').textContent = 'Collapse report'; size();
          }
          el('full').addEventListener('click', function () {
            var want = displayMode === 'fullscreen' ? 'inline' : 'fullscreen';
            send('ui/request-display-mode', { mode: want }, 5000).then(function (res) {
              var got = res && res.mode;
              if (got) { setMode(got); }
              if (want === 'fullscreen' && got !== 'fullscreen') { expandInline(); }
            }, function () { if (want === 'fullscreen') { expandInline(); } });
          });
          send('ui/initialize', {
            appInfo: { name: 'azurecatalyst-business-case-report', version: '0.3.0' },
            appCapabilities: { availableDisplayModes: ['inline', 'fullscreen'] }, protocolVersion: '2025-06-18'
          }, 8000)
            .then(function (res) {
              if (res && res.hostContext) { applyHostContext(res.hostContext); }
              window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/initialized', params: {} }, '*');
            }, function () { })
            .then(size);
        })();
        </script>
        </body>
        </html>
        """;
}

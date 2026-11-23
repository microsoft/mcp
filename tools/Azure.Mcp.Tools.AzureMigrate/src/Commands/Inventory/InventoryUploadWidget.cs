// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.McpApps;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC: MCP Apps upload widget for the inventory create-or-update tool. The host renders it with the tool result; it reads
/// the write-only SAS URL from 'uploadCommand', lets the user pick the file, PUTs it straight to blob storage from the browser
/// (needs storage CORS for the host's widget origin). It does not track the inventory: the agent waits for it and then calls the
/// status tool once, whose card shows the summary together with the agent's reply, so the panel never shows a stale state.
/// Enabled only when <see cref="DomainsSetting"/> lists the storage origins the widget may connect to.
/// </summary>
internal static class InventoryUploadWidget
{
    internal const string ResourceUri = "ui://azuremigrate/inventory-upload-v2.html";

    /// <summary>Comma-separated origins (e.g. https://acct.blob.core.windows.net) the widget may connect to.</summary>
    internal const string DomainsSetting = "AZMIGRATE_INVENTORY_WIDGET_DOMAINS";

    internal static IReadOnlyList<string> ConnectDomains =>
        (Environment.GetEnvironmentVariable(DomainsSetting) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal static bool IsEnabled => ConnectDomains.Count > 0;

    /// <summary>Registers the widget for the create-or-update command when enabled.</summary>
    internal static void Register(string commandId)
    {
        if (IsEnabled)
        {
            McpAppRegistry.Register(commandId, new McpAppWidget(ResourceUri, "Azure Catalyst inventory upload", Html, ConnectDomains));
        }
    }

    private const string Html = """
        <!doctype html>
        <html>
        <head><meta charset="utf-8"><title>Inventory upload</title>
        <style>
        body{font:13px Segoe UI,sans-serif;margin:8px;color:#222}
        .row{margin:8px 0}
        progress{width:100%}
        .muted{color:#666}
        .ok{color:#107c10}.err{color:#a4262c}
        pre{white-space:pre-wrap;word-break:break-word;margin:0;max-height:200px;overflow:auto;background:#f4f4f4;padding:4px}
        #summary p{margin:4px 0}#summary ul{margin:4px 0;padding-left:20px}#summary code{background:#f4f4f4;padding:0 2px}
        </style>
        </head>
        <body>
        <b id="title">Azure Catalyst inventory</b>
        <div class="muted" id="hint">Starting...</div>
        <div class="row" id="filerow" style="display:none"><input type="file" id="file" disabled accept=".zip,.csv,.xlsx,.xls,.json,.txt"></div>
        <div class="row" id="progrow" style="display:none"><progress id="prog" max="100" value="0"></progress></div>
        <div id="status"></div>
        <div class="row" id="inv"></div>
        <div class="row" id="summary"></div>
        <details><summary class="muted">Details</summary><pre id="diag"></pre></details>
        <script>
        (function () {
          var MAX_BYTES = 500 * 1024 * 1024;
          var ctx = { uploadUrl: null, sessionId: null, operationId: null };
          var diag = { origin: location.origin, events: [] };
          var el = function (id) { return document.getElementById(id); };
          var fileEl = el('file'), progEl = el('prog');
          document.querySelector('details').addEventListener('toggle', function () { size(); });
          // documentElement.scrollHeight never drops below the frame's current height, so measure the body's content instead.
          function size() {
            var h = Math.ceil(document.body.getBoundingClientRect().height) + 16;
            window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/size-changed', params: { width: document.documentElement.scrollWidth, height: h } }, '*');
          }
          function show(id, text, cls) { var e = el(id); e.textContent = text; e.className = cls || ''; el('diag').textContent = JSON.stringify(diag, null, 2); size(); }
          var nextId = 1, pending = {};
          function send(method, params, timeoutMs) {
            var id = nextId++;
            window.parent.postMessage({ jsonrpc: '2.0', id: id, method: method, params: params }, '*');
            return new Promise(function (resolve, reject) {
              pending[id] = { resolve: resolve, reject: reject };
              setTimeout(function () { if (pending[id]) { delete pending[id]; reject(new Error('timeout ' + method)); } }, timeoutMs || 15000);
            });
          }
          // Returns the tool's structured result: the first object with a 'sessionId', parsing JSON text content on the way.
          function findResult(node, depth) {
            if (depth > 8 || node === null || node === undefined) { return null; }
            if (typeof node === 'string') {
              var s = node.trim();
              if (s.charAt(0) === '{' || s.charAt(0) === '[') { try { return findResult(JSON.parse(s), depth + 1); } catch (e) { return null; } }
              return null;
            }
            if (typeof node !== 'object') { return null; }
            // Null fields are omitted, so a text-only result has no 'uploadCommand'; 'success' identifies it.
            if (typeof node.sessionId === 'string' && ('success' in node || 'uploadCommand' in node || 'isCompleted' in node)) { return node; }
            if (node.success === false && 'error' in node) { return node; }
            for (var k in node) { var r = findResult(node[k], depth + 1); if (r) { return r; } }
            return null;
          }
          function onToolResult(params) {
            var r = findResult(params, 0);
            diag.toolResult = r ? { success: r.success, sessionId: r.sessionId, operationId: r.operationId, hasUploadCommand: !!r.uploadCommand } : 'not found';
            if (!r || ctx.uploadUrl) { show('status', ''); return; }
            if (!r.success) { show('hint', 'The inventory could not be started. See the chat for details.', 'err'); return; }
            ctx.sessionId = r.sessionId; ctx.operationId = r.operationId;
            var m = /"(https:\/\/[^"]+)"\s+--from-to/.exec(r.uploadCommand || '');
            if (!m) {
              // The host renders the panel on every call of the tool, including text-only updates. Show progress, not a file picker.
              el('title').textContent = 'Updating your inventory';
              show('hint', 'Applying your change; nothing needs to be uploaded. Copilot posts the result in the chat when it is done.', 'muted');
              return;
            }
            ctx.uploadUrl = m[1];
            diag.storageOrigin = ctx.uploadUrl.split('/').slice(0, 3).join('/');
            el('title').textContent = 'Upload your inventory file';
            el('filerow').style.display = ''; el('progrow').style.display = '';
            fileEl.disabled = false;
            show('hint', 'Choose your CMDB, CSV or Excel export (up to 500 MB) within ' + (r.uploadWindowMinutes || 30) + ' minutes. It goes straight to Azure storage.', 'muted');
          }
          window.addEventListener('message', function (e) {
            var m = e.data;
            if (!m || m.jsonrpc !== '2.0') { return; }
            if (m.method) {
              diag.events.push(m.method);
              if (m.method === 'ui/notifications/tool-result') { onToolResult(m.params); }
              if (m.id !== undefined) { window.parent.postMessage({ jsonrpc: '2.0', id: m.id, result: {} }, '*'); }
            } else if (m.id !== undefined && pending[m.id]) {
              var p = pending[m.id]; delete pending[m.id];
              if (m.error) { p.reject(new Error(JSON.stringify(m.error))); } else { p.resolve(m.result); }
            }
          });

          fileEl.addEventListener('change', function () {
            var f = fileEl.files && fileEl.files[0];
            if (!f) { return; }
            if (f.size > MAX_BYTES) { show('status', f.name + ' is larger than 500 MB.', 'err'); return; }
            fileEl.disabled = true;
            diag.file = { name: f.name, size: f.size };
            var started = Date.now();
            // A single Put Blob straight to the write-only SAS URL, like the portal's CSV import. The file never passes through
            // the MCP server or the chat. This needs a CORS rule on the storage account for the host's widget origin.
            var xhr = new XMLHttpRequest();
            xhr.open('PUT', ctx.uploadUrl);
            xhr.setRequestHeader('x-ms-blob-type', 'BlockBlob');
            xhr.setRequestHeader('content-type', f.type || 'application/octet-stream');
            xhr.upload.onprogress = function (ev) { if (ev.lengthComputable) { progEl.value = Math.round(ev.loaded * 100 / ev.total); } };
            xhr.onload = function () {
              diag.put = { status: xhr.status, ms: Date.now() - started };
              if (xhr.status === 201) {
                progEl.value = 100;
                show('status', 'Uploaded ' + f.name + '. Building your inventory; Copilot posts the result in the chat when it is ready.', 'ok');
              } else {
                fileEl.disabled = false;
                show('status', 'Upload failed (HTTP ' + xhr.status + '). Try again, or ask for the command-line upload instead.', 'err');
              }
            };
            xhr.onerror = function () {
              diag.put = { status: 'network error' };
              fileEl.disabled = false;
              show('status', 'Upload was blocked by the browser (network or CORS). Ask for the command-line upload instead.', 'err');
            };
            show('status', 'Uploading ' + f.name + '...');
            xhr.send(f);
          });

          show('status', '');
          send('ui/initialize', { appInfo: { name: 'azmigrate-inventory-upload', version: '0.1.0' }, appCapabilities: {}, protocolVersion: '2025-06-18' }, 8000)
            .then(function () { diag.init = 'ok'; window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/initialized', params: {} }, '*'); },
                  function (err) { diag.init = 'failed: ' + err.message; })
            .then(function () { show('status', ''); });
        })();
        </script>
        </body>
        </html>
        """;
}

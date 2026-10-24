# Azure Migrate MCP Toolset

The Azure Migrate toolset provides Platform Landing Zone generation and configuration guidance.

## Recorded tests

For the shared test infrastructure and recording workflow, see the [recorded-test guide](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md).

### Platform Landing Zone downloads

The create/wait/download test regenerates the existing `default` landing zone, retaining its effective
configuration. Obtain permission for the configured project before recording; the test refuses to
overwrite an in-flight or unknown generation state. It downloads and reads the generated ZIP but does
not deploy its contents.

Before publishing, verify the live ZIP size and SHA256 against `metadata.json` from the newly committed
artifact version, using the `artifactId` returned by the PLZ resource. Do not reuse a previous run's
manifest. The recorded test checks every ZIP entry can be read and that Terraform files are present.

The test's text sanitizers remove live SAS URLs, project identities and configuration identifiers.
The sanitized download URL uses a public IP placeholder, which the proxy serves during playback.
This keeps public-target validation enabled without resolving a fake storage hostname or accessing
the placeholder endpoint.
**The proxy does not sanitize inside binary ZIP bodies.** Inspect every archived file before pushing
assets, sanitize identities and secrets in both entry names and contents, and rebuild the recorded ZIP
with valid sizes and checksums. Preserve representative generated content and verify playback after
this step. The sanitized archive has a different hash from the live artifact; never use it as evidence
of the live manifest match.

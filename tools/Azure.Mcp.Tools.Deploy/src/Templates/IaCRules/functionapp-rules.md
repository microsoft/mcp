=== Additional requirements for Function Apps:
- Attach User-Assigned Managed Identity.
{{ToolSpecificRules}}
- Do not add `node_modules` to `.funcignore` when using the Flex Consumption plan. This must not be ignored.
- Include `function.json` files. This is part of the older paradigm, but Copilot may not yet support the newer approach fully.

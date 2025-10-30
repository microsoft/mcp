# Regenerating this code

- Add this file to the repo under `eng/`
  ```json
  {
    "main": "dist/src/index.js",
    "dependencies": {
      "@typespec/http-client-csharp": "latest"
    },
    "devDependencies": {
      "@azure-tools/typespec-autorest": "0.60.0",
      "@azure-tools/typespec-azure-core": "0.60.0",
      "@azure-tools/typespec-azure-resource-manager": "0.60.0",
      "@azure-tools/typespec-azure-rulesets": "0.60.0",
      "@azure-tools/typespec-client-generator-core": "0.60.0",
      "@azure-tools/typespec-liftr-base": "0.8.0",
      "@typespec/compiler": "1.4.0",
      "@typespec/events": "0.74.0",
      "@typespec/http": "1.4.0",
      "@typespec/openapi": "1.4.0",
      "@typespec/rest": "0.74.0",
      "@typespec/sse": "0.74.0",
      "@typespec/streams": "0.74.0",
      "@typespec/versioning": "0.74.0",
      "@typespec/xml": "0.74.0"
    }
  }
  ```
- Then install `tsp-client` to your NODE installation globally: `npm install -g @azure-tools/typespec-client-generator-cli`
- `tsp-client update` from within the directory containing the `tsp-location.yaml` file.
- This will regenerate the C# client code that resides in this directory.

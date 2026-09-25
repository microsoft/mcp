# Paginated Report definition

This article provides a breakdown of the definition structure for Paginated Report items.

## Supported formats

Paginated Report definitions use the `PaginatedReportDefinition` format. The `PaginatedReportDefinition` format is based on the RDL (Report Definition Language) XML format encoded as Base64 string.

## Definition parts

The definition of a Paginated Report item with `PaginatedReportDefinition` format is constructed from one required Report Definition Language (RDL) part (and may also include an optional `.platform` part):
* **Path** - The RDL file name, for example: `SamplePaginatedReport.rdl`
* **Payload** - The RDL file content, Base64-encoded. See [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64).
* **Payload type** - `InlineBase64`

This table lists the Paginated Report definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `<reportName>.rdl` | Report Definition Language (RDL) | true | The paginated report content in [RDL format](https://learn.microsoft.com/sql/reporting-services/reports/report-definition-language-ssrs?view=sql-server-ver17), encoded as Base64. The file name must match the display name of the report (for example: if `displayName` is `ContosoReport`, the path should be `ContosoReport.rdl`). |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item. Typically system-generated and returned by the service. |

## Definition example

Here's an example of an item definition for a Paginated Report. The example includes the optional `.platform` part, which is typically system-generated and returned by the service.

```json
{
    "format": "PaginatedReportDefinition",
    "parts": [
        {
            "path": "SamplePaginatedReport.rdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": ".platform",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
    ]
}
```

## Report Definition Language (RDL) definition part

### Path

The path contains the full RDL filename suffixed with `.rdl`. It should match exactly the display name of the artifact.
### Payload

The payload of the RDL definition part contains the paginated report content in [RDL (Report Definition Language)](https://learn.microsoft.com/sql/reporting-services/reports/report-definition-language-ssrs?view=sql-server-ver17) format, encoded as Base64. See the [RDL Schema Specification](https://learn.microsoft.com/openspecs/sql_server_protocols/ms-rdl/53287204-7cd0-4bc9-a5cd-d42a5925dca1) for the full schema reference.

### Payload type

`InlineBase64`

### Example of payload content decoded from Base64

```xml
<?xml version="1.0" encoding="utf-8"?>
<Report MustUnderstand="df" xmlns="http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition" xmlns:rd="http://schemas.microsoft.com/SQLServer/reporting/reportdesigner" xmlns:df="http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition/defaultfontfamily" xmlns:am="http://schemas.microsoft.com/sqlserver/reporting/authoringmetadata">
  <rd:ReportUnitType>Inch</rd:ReportUnitType>
  <rd:ReportID>89cb7e1e-800d-4ef5-8057-1d4f8b5bd12e</rd:ReportID>
  <am:AuthoringMetadata>
    <am:CreatedBy>
      <am:Name>PBIRB</am:Name>
      <am:Version>15.7.1818.92</am:Version>
    </am:CreatedBy>
    <am:UpdatedBy>
      <am:Name>PBIRB</am:Name>
      <am:Version>15.7.1818.92</am:Version>
    </am:UpdatedBy>
    <am:LastModifiedTimestamp>2026-06-18T07:20:20.3631951Z</am:LastModifiedTimestamp>
  </am:AuthoringMetadata>
  <df:DefaultFontFamily>Segoe UI</df:DefaultFontFamily>
  <AutoRefresh>0</AutoRefresh>
  <ReportSections>
    <ReportSection>
      <Body>
        <ReportItems>
          <Textbox Name="ReportTitle">
            <rd:WatermarkTextbox>Title</rd:WatermarkTextbox>
            <rd:DefaultName>ReportTitle</rd:DefaultName>
            <CanGrow>true</CanGrow>
            <KeepTogether>true</KeepTogether>
            <Paragraphs>
              <Paragraph>
                <TextRuns>
                  <TextRun>
                    <Value>Sample RDL Text</Value>
                    <Style>
                      <FontFamily>Segoe UI Light</FontFamily>
                      <FontSize>28pt</FontSize>
                    </Style>
                  </TextRun>
                </TextRuns>
                <Style />
              </Paragraph>
            </Paragraphs>
            <Height>0.5in</Height>
            <Width>5.5in</Width>
            <Style>
              <Border>
                <Style>None</Style>
              </Border>
              <PaddingLeft>2pt</PaddingLeft>
              <PaddingRight>2pt</PaddingRight>
              <PaddingTop>2pt</PaddingTop>
              <PaddingBottom>2pt</PaddingBottom>
            </Style>
          </Textbox>
        </ReportItems>
        <Height>2.25in</Height>
        <Style>
          <Border>
            <Style>None</Style>
          </Border>
        </Style>
      </Body>
      <Width>6in</Width>
      <Page>
        <PageFooter>
          <Height>0.45in</Height>
          <PrintOnFirstPage>true</PrintOnFirstPage>
          <PrintOnLastPage>true</PrintOnLastPage>
          <ReportItems>
            <Textbox Name="ExecutionTime">
              <rd:DefaultName>ExecutionTime</rd:DefaultName>
              <CanGrow>true</CanGrow>
              <KeepTogether>true</KeepTogether>
              <Paragraphs>
                <Paragraph>
                  <TextRuns>
                    <TextRun>
                      <Value>=Globals!ExecutionTime</Value>
                      <Style />
                    </TextRun>
                  </TextRuns>
                  <Style>
                    <TextAlign>Right</TextAlign>
                  </Style>
                </Paragraph>
              </Paragraphs>
              <Top>0.2in</Top>
              <Left>4in</Left>
              <Height>0.25in</Height>
              <Width>2in</Width>
              <Style>
                <Border>
                  <Style>None</Style>
                </Border>
                <PaddingLeft>2pt</PaddingLeft>
                <PaddingRight>2pt</PaddingRight>
                <PaddingTop>2pt</PaddingTop>
                <PaddingBottom>2pt</PaddingBottom>
              </Style>
            </Textbox>
          </ReportItems>
          <Style>
            <Border>
              <Style>None</Style>
            </Border>
          </Style>
        </PageFooter>
        <LeftMargin>1in</LeftMargin>
        <RightMargin>1in</RightMargin>
        <TopMargin>1in</TopMargin>
        <BottomMargin>1in</BottomMargin>
        <Style />
      </Page>
    </ReportSection>
  </ReportSections>
  <ReportParametersLayout>
    <GridLayoutDefinition>
      <NumberOfColumns>4</NumberOfColumns>
      <NumberOfRows>2</NumberOfRows>
    </GridLayoutDefinition>
  </ReportParametersLayout>
</Report>
```

## Platform part

The `.platform` part contains platform metadata in JSON format, encoded as Base64. This part is optional in requests and is typically generated by the service.

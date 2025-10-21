---
title: DataPipeline definition
description: Learn how to structure a data pipeline item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric data pipeline item definition
author: rnandurimsft
ms.author: rnanduri
ms.service: fabric
ms.date: 02/19/2025
---

# DataPipeline definition

This article provides a breakdown of the definition structure for data pipeline items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `pipeline-content.json` | ContentDetails (JSON) | true | Describes data pipeline content of payload |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

## ContentDetails

Describes content of payload

| Name                  | Type            | Description                         |
|-----------------------|-----------------|-------------------------------------|
| properties            | DataPipelineProperties            | DataPipeline properties. See [Description for DataPipelineProperties contents](#description-for-datapipelineproperties-contents)|

### Description for DataPipelineProperties contents

Describes the fields used to construct the DataPipelineProperties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| activities            | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[]      | false           | List of activities.|
| description           | String          | false           | Description of Data Pipeline |

### Description for DataPipelineActivity contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| name                  | String          | true            | Name of the activity    |
| type                  | [DataPipelineActivityType](#datapipelineactivitytypes)          | true            | Type of the activity    |
| state                 | [ActivityState](#activitystate-values)   |  false             | State of the activity. Set as `Active` by default. |
| onInactiveMarkAs      | [OnInactiveMarkAs](#oninactivemarkas-values) | false  | Status result of the activity when the state is set to Inactive. The status will be `Succeeded` by default if not set.|
| dependsOn             | [DependencyActivity](#description-for-dependencyactivity-contents)[]           | false           | Array of activities or conditions the activity depends on. See [Description for DependencyActivity contents](#description-for-dependencyactivity-contents) |
| typeProperties        | [Activity Properties](#activity-properties)    | true            | Type-specific properties of the activity. The structure varies depending on the activity type. See [Activity Properties](#activity-properties) for details. |
| policy                | [ActivityPolicy](#activity-policy) | false | Execution Policy for an activity. Refer to the [Pipeline Activities List](#datapipelineactivitytypes) for the types which support this property. |
| externalReferences    | [External Reference](#external-references) | false | Refer to the [Pipeline Activities List](#datapipelineactivitytypes) for the types which support this property.|

### ActivityState Values

| Name                  | Type            | Description       |
|-----------------------|-----------------|-------------------|
| Active                | String          | Default Activity State.|
| InActive              | String          | Marks the activity as inactive and skips execution for it.|

### OnInactiveMarkAs Values

| Name                  | Type            | Description       |
|-----------------------|-----------------|-------------------|
| Succeeded             | String          | Default Value.    |
| Failed                | String          | Always mark inactive activity as failed. |
| Skipped               | String          | Inactive activity is skipped.|

### Description for DependencyActivity contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| activity              | String          | true            | Name of the activity    |
| dependencyConditions  | [Conditions](#description-for-conditions-contents)| true    | List of Dependency Conditions.|

### Description for Conditions contents

| Name                  | Type            |
|-----------------------|-----------------|
| Succeeded             | String          |
| Failed                | String          |
| Skipped               | String          |
| Completed             | String          |

### Activity Policy

Execution policy for an activity.

| Name                  | Type            | Required | Description       |
|-----------------------|-----------------|----------|-------------------|
| timeout               | String (or Expression with resultType string)         | false    | Specifies the timeout for the activity to run. The default timeout is 7 days. |
| retry                 | integer (or Expression with resultType integer)          | false    | Maximum ordinary retry attempts. Default is 0. Minimum: 0. |
| retryIntervalInSeconds| Integer         | false    | Interval between each retry attempt (in seconds). The default is 30 sec. Minimum: 30, Maximum: 86400. |
| secureInput           | Boolean         | false    | When set to true, Input from activity is considered as secure and will not be logged to monitoring. |
| secureOutput          | Boolean         | false    | When set to true, Output from activity is considered as secure and will not be logged to monitoring. |

### External References

External references to connection.

| Name                  | Type            | Required | Description       |
|-----------------------|-----------------|----------|-------------------|
| connection            | String (Guid)   | true     | Connection Id of the referenced connection. |

### DataPipelineActivityTypes

| Name                                                             | Description                                                   |
|------------------------------------------------------------------|---------------------------------------------------------------|
| [Copy](#copy-activity-properties)                               | Copy Activity that copies data from a source to a destination |
| [AzureHDInsight](#azurehdinsight-activity-properties)           | Runs various programs (Hive, Pig, MapReduce, Streaming, Spark) on an Azure HDInsight cluster |
| [SparkJobDefinition](#sparkjobdefinition-activity-properties)   | Executes a Spark job definition                               |
| [InvokeCopyJob](#invokecopyjob-activity-properties)             | Invokes a copy job activity                                   |
| [ExecuteSSISPackage](#executessispackage-activity-properties)   | Executes a SQL Server Integration Services (SSIS) package    |
| [SqlServerStoredProcedure](#sqlserverstoredprocedure-activity-properties) | Executes a stored procedure in SQL Server                     |
| [InvokePipeline](#invokepipeline-activity-properties)           | Invokes another pipeline (deprecated, use ExecutePipeline)    |
| [ExecutePipeline](#execute-pipeline-activity-properties)        | Executes another pipeline as a nested activity                |
| [Delete](#delete-activity-properties)                           | Deletes data from a data source                               |
| [KustoQueryLanguage](#kustoquerylanguage-activity-properties)   | Executes a KQL query on Azure Data Explorer                  |
| [Lookup](#lookup-activity-properties)                           | Retrieves data from a data source for use in subsequent activities |
| [WebActivity](#web-activity-properties)                         | Makes HTTP requests to external web services                  |
| [GetMetadata](#getmetadata-activity-properties)                 | Retrieves metadata information from a data source             |
| [IfCondition](#if-condition-activity-properties)                | Executes activities based on a conditional expression         |
| [Switch](#switch-activity-properties)                           | Executes different activities based on a switch expression    |
| [ForEach](#foreach-activity-properties)                         | Iterates over a collection and executes activities for each item |
| [AzureMLExecutePipeline](#azureml-activity-properties)                         | Executes Azure Machine Learning operations (batch execution, update resource, execute pipeline) |
| [DataLakeAnalyticsScope](#datalakeanalyticsscope-activity-properties) | Runs a Scope script on Azure Data Lake Analytics             |
| [Wait](#wait-activity-properties)                               | Pauses the pipeline execution for a specified duration        |
| [Fail](#fail-activity-properties)                               | Explicitly fails the pipeline with a specified error message  |
| [Until](#until-activity-properties)                             | Repeats activities until a condition is met                   |
| [Filter](#filter-activity-properties)                           | Filters an array based on a provided condition               |
| [TridentNotebook](#tridentnotebook-activity-properties)         | Executes a Trident notebook                                   |
| [DatabricksNotebook](#databricksnotebook-activity-properties)   | Executes Databricks operations (notebook, Spark JAR, Spark Python) |
| [SetVariable](#setvariable-activity-properties)                 | Sets the value of an existing variable                        |
| [AppendVariable](#appendvariable-activity-properties)           | Appends a value to an existing array variable                 |
| [AzureFunction](#azurefunction-activity-properties)             | Executes an Azure Function                                    |
| [Custom](#custom-activity-properties)                           | Azure Batch - Executes a custom activity with user-defined command          |
| [WebHook](#webhook-activity-properties)                         | Calls a webhook and waits for a callback                     |
| [RefreshDataFlow](#refreshdataflow-activity-properties)         | Refreshes a data flow                                         |
| [Script](#script-activity-properties)                           | Executes custom scripts (PowerShell, Python, etc.)          |
| [Office365Email](#office365email-activity-properties)          | Sends an email using Office 365                              |
| [Email](#email-activity-properties)                             | Sends an email notification                                   |
| [MicrosoftTeams](#microsoftteams-activity-properties)           | Sends a message to Microsoft Teams                            |
| [Teams](#teams-activity-properties)                             | Sends a message to Teams                                      |
| [PBISemanticModelRefresh](#pbisemanticmodelrefresh-activity-properties) | Refreshes a Power BI semantic model                          |

## Activity Properties

The `typeProperties` field in each activity contains type-specific configuration that varies based on the activity type. This section describes the properties required for each activity types.

### Wait Activity Properties

Properties for activities with `type: "Wait"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [WaitActivityTypeProperties](#wait-activity-type-properties) | true | The properties for wait activity. |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|

#### Wait Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| waitTimeInSeconds     | Integer         | true            | The number of seconds to wait before proceeding to the next activity |

### Copy Activity Properties

Properties for activities with `type: "Copy"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [CopyActivityTypeProperties](#copy-activity-type-properties) | true   | The properties for copy activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution policy for the activity.|

### Copy Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| source                | [CopySource](#copysource)      | true            | Source settings for the copy operation |
| sink                  | [CopySink](#copysink)        | false            | Sink settings for the copy operation |
| destination           | [CopySink](#copysink)        | false            | Copy activity destination |
| translator            | Object | false           | Copy activity translator. If not specified, tabular translator is used |
| enableStaging         | Boolean         | false           | Specifies whether to copy data via an interim staging. Default value is false |
| stagingSettings       | [StagingSettings](#stagingsettings) | false           | Specifies interim staging settings when EnableStaging is true |
| scriptLines           | String[]        | false           | Data flow script lines, used when copy runs as data flow |
| caller                | String          | false           | The name of the 'caller' used in ADMS Telemetry |
| linkedIntegrationRuntime | Object       | false           | Reference to linked integration runtime in mounted data factory |
| parallelCopies        | Integer         | false           | Maximum number of concurrent sessions opened on the source or sink to avoid overloading the data store. Minimum: 0 |
| dataIntegrationUnits  | Integer         | false           | Maximum number of data integration units that can be used to perform this data movement. Minimum: 0 |
| throughputOptimizationUnits | Integer   | false           | Maximum number of throughput optimization units that can be used to perform this data movement. Minimum: 0 |
| enableSkipIncompatibleRow | Boolean     | false           | Whether to skip incompatible row. Default value is false |
| redirectIncompatibleRowSettings | Object | false           | Redirect incompatible row settings when EnableSkipIncompatibleRow is true |
| logStorageSettings    | Object          | false           | Log storage settings customer need to provide when enabling session log |
| logSettings           | Object          | false           | Log settings customer needs provide when enabling log |
| preserveRules         | String[]        | false           | Preserve Rules. |
| preserve              | String[]        | false           | Preserve rules. |
| resumeId              | String          | false           | The resumeId to enable copy resumability |
| validateDataConsistency | Boolean       | false           | Whether to enable Data Consistency validation |
| skipErrorFile         | Object          | false           | Specify the fault tolerance for data consistency |

#### CopySource

| Name                          | Type            | Required        | Description       |
|-------------------------------|-----------------|-----------------|-------------------|
| type                          | String          | true            | Copy source type |
| sourceRetryCount              | Integer         | false          | Source retry count.|
| sourceRetryWait               | String          | false           | Source retry wait.|
| maxConcurrentConnections      | Integer         | false           | The maximum concurrent connection count for the source data store.|
| disableMetricsCollection      | Boolean         | false           | If true, disable data store metrics collection. Default is false.|
| datasetSettings               | [DatasetSettings](#datasetsettings)          | false           | Delete activity dataset settings |
| checkpointProperties          | Object          | false           | Checkpoint Properties |
| genericChangeDataProperties   | Object          | false           | Generic Change Data Capture Properties |

#### CopySink

| Name                          | Type            | Required        | Description       |
|-------------------------------|-----------------|-----------------|-------------------|
| type                          | String          | true            | Copy sink type. |
| writeBatchSize                | Integer         | false           | Write batch size, minimum: 0. |
| writeBatchTimeout             | String          | false           | Write batch timeout.|
| sinkRetryCount                | Integer         | false           | Sink retry count.|
| sinkRetryWait                 | String          | false           | Sink retry wait. |
| maxConcurrentConnections      | Integer         | false           | The maximum concurrent connection count for the sink data store.|
| disableMetricsCollection      | Boolean         | false           | If true, disable data store metrics collection. Default is false.|
| datasetSettings               | [DatasetSettings](#datasetsettings)          | false           | dataset settings |

#### CopyTranslator

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Type of translator (e.g., "TabularTranslator") |
| typeConversion        | Boolean         | false           | Whether to enable advanced type conversion property for copy activity.|
| typeConversionSettings| [TypeConversionSettings](#typeconversionsettings)          | false           | Type Conversion Settings.|
| columnMappings        | String          | false           | Column mappings.|
| schemaMapping         | String          | false           | The schema mapping to map between tabular data and hierarchical data.|
| mappings              | String          | false           | Column mappings with logical types.|
| columnFlatteningSettings| Object        | false           | Column Flattening Settings.|

#### TypeConversionSettings

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| allowDataTruncation   | Boolean         | false           | Whether to allow data truncation when converting the data.|
| treatBooleanAsNumber  | Boolean         | false           | Whether to treat boolean values as numbers.|
| dateTimeFormat        | String          | false           | The format for DateTime values.|
| dateTimeOffsetFormat  | String          | false           | The format for DateTimeOffset values.|
| timeSpanFormat        | String          | false           | The format for TimeSpan values.|
| culture               | String          | false           | The culture used to convert data from/to string.|

#### StagingSettings

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| enableCompression     | Boolean         | false           | Specifies whether to use compression when copying data via an interim staging. |
| path                  | String          | false           | Path for staging data |
| externalReferences    | [ExternalReferences](#external-references) | true | External references to a connection.|

#### DatasetSettings

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Type of dataset |
| description           | String          | false           | Dataset description |
| structure             | Object          | false           | Columns that define the structure of the dataset. Type: array (or Expression with resultType array), itemType: DatasetDataElement |
| schema                | Object          | false           | Columns that define the physical type schema of the dataset. Type: array (or Expression with resultType array), itemType: DatasetSchemaDataElement |
| copyJobProperties     | Object          | false           | Additional properties in the dataset for CopyJob System Pipelines |
| connectionProperties  | Object          | false           | The connectionProperties setting options. Type: key value pairs (value should be string type) (or Expression with resultType object) |
| externalReferences    | [ExternalReferences](#external-references)          | false           | External references to connection |
| linkedServiceName     | Object          | false           | Linked service reference |
| linkedService         | Object          | false           | Linked service |
| connectionSettings    | Object          | false           | Connection Settings |
| parameters            | Object          | false           | Parameters for dataset |
| annotations           | Array           | false           | List of tags that can be used for describing the Dataset |
| folder                | Object          | false           | The folder that this Dataset is in. If not specified, Dataset will appear at the root level |

### Lookup Activity Properties

Properties for activities with `type: "Lookup"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [LookupActivityTypeProperties](#lookup-activity-type-properties) | true   | The properties for lookup activity |

#### Lookup Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| source                | [CopySource](#copysource)    | true            | Source settings for the lookup operation |
| datasetSettings       | [DatasetSettings](#datasetsettings)          | true            | Dataset reference for the lookup |
| firstRowOnly          | Boolean         | false           | Whether to return only the first row (default: true) |

### Web Activity Properties

Properties for activities with `type: "WebActivity"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [WebActivityTypeProperties](#web-activity-type-properties) | true   | The properties for lookup activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the connection used.|

#### Web Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| relativeUrl           | String          | true            | URL to call |
| method                | String          | true            | HTTP method (GET, POST, PUT, DELETE) |
| headers               | String          | false           | HTTP headers |
| body                  | String          | false           | Request body (for POST/PUT requests) |
| disableCertValidation | Boolean         | false           | When set to true, Certificate validation will be disabled. |
| httpRequestTimeout    | String          | false           | Request timeout. Format is in timespan(hh:mm:ss) |
| turnOffAsync          | Boolean         | false           | When set to true, stops invoking HTTP GET on http location given in response header.|

### If Condition Activity Properties

Properties for activities with `type: "IfCondition"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [IfConditionActivityTypeProperties](#if-condition-activity-type-properties) | true   | The properties for if condition activity |

#### If Condition Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| expression            | [Expression](#expression)      | true            | Boolean expression to evaluate |
| ifTrueActivities      | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | false  | Activities to execute if condition is true |
| ifFalseActivities     | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | false  | Activities to execute if condition is false |

#### Expression

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Expression type (e.g., "Expression") |
| value                 | String          | true            | Expression value |

### ForEach Activity Properties

Properties for activities with `type: "ForEach"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [ForEachActivityTypeProperties](#foreach-activity-type-properties) | true   | The properties for foreach activity |

#### ForEach Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| items                 | [Expression](#expression)      | true            | Expression that returns an array to iterate over |
| activities            | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | true            | Activities to execute for each item |
| isSequential          | Boolean         | false           | Whether to execute iterations sequentially (default: false) |
| batchCount            | Integer         | false           | Number of concurrent iterations (when isSequential is false) |

### Execute Pipeline Activity Properties

Properties for activities with `type: "ExecutePipeline"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [ExecutePipelineActivityTypeProperties](#execute-pipeline-activity-type-properties) | true   | The properties for execute pipeline activity |
| policy                | [ActivityPolicy](#activity-policy)| true | Activity Policy.|

#### Execute Pipeline Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| pipeline              | [PipelineReference](#pipelinereference) | true            | Reference to the pipeline to execute |
| parameters            | object          | false           | Parameters to pass to the child pipeline |
| waitOnCompletion      | Boolean         | false           | Whether to wait for child pipeline completion (default: true) |

##### PipelineReference

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| referenceName         | String          | true            | Name of the referenced pipeline |
| type                  | String          | true            | Reference type (typically "PipelineReference") |

### Fail Activity Properties

Properties for activities with `type: "Fail"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [FailActivityTypeProperties](#fail-activity-type-properties) | true   | The properties for fail activity |

#### Fail Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| message               | String          | true            | The error message that surfaced in the Fail activity. It can be dynamic content that's evaluated to a non empty/blank string at runtime. |
| errorCode             | String          | true            | The error code that categorizes the error type of the Fail activity. It can be dynamic content that's evaluated to a non empty/blank string at runtime. |

### Filter Activity Properties

Properties for activities with `type: "Filter"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [FilterActivityTypeProperties](#filter-activity-type-properties) | true   | The properties for filter activity |

#### Filter Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| items                 | [Expression](#expression)      | true            | Input array on which filter should be applied. |
| condition             | [Expression](#expression)      | true            | Condition to be used for filtering the input. |

### Until Activity Properties

Properties for activities with `type: "Until"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [UntilActivityTypeProperties](#until-activity-type-properties) | true   | The properties for until activity |

#### Until Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| expression            | [Expression](#expression)      | true            | An expression that would evaluate to Boolean. The loop will continue until this expression evaluates to true. |
| activities            | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | true            | List of activities to execute. |
| timeout               | String (or Expression with resultType string)         | false           | Specifies the timeout for the activity to run. If there is no value specified, it takes the value of TimeSpan.FromDays(7) which is 1 week as default. |

### Switch Activity Properties

Properties for activities with `type: "Switch"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [SwitchActivityTypeProperties](#switch-activity-type-properties) | true   | The properties for switch activity |

#### Switch Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| on                    | [Expression](#expression)      | true            | An expression that would evaluate to a string or integer. This is used to determine the block of activities in cases that will be executed. |
| cases                 | [SwitchCase](#switchcase)[]    | false           | List of cases that correspond to expected values of the 'on' property. This is an optional property and if not provided, the activity will execute activities provided in defaultActivities. |
| defaultActivities     | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | false           | List of activities to execute if no case condition is satisfied. This is an optional property and if not provided, the activity will exit without any action. |

#### SwitchCase

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| value                 | String          | true            | Expected value of expression result used for case selection. |
| activities            | [DataPipelineActivity](#description-for-datapipelineactivity-contents)[] | true            | List of activities to execute for matched case. |

### GetMetadata Activity Properties

Properties for activities with `type: "GetMetadata"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [GetMetadataActivityTypeProperties](#getmetadata-activity-type-properties) | true   | The properties for getmetadata activity |

#### GetMetadata Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| datasetSettings       | Object          | true            | GetMetadata activity dataset settings. |
| fieldList             | String[]        | false           | Fields of metadata to get from dataset. Type: string (or Expression with resultType string). |
| storeSettings         | Object          | false           | GetMetadata activity store settings. |
| formatSettings        | Object          | false           | GetMetadata activity format settings. |

### SetVariable Activity Properties

Properties for activities with `type: "SetVariable"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [SetVariableActivityTypeProperties](#setvariable-activity-type-properties) | true   | The properties for setvariable activity |
| policy                | [SecureInputOutputPolicy](#secureinputoutputpolicy) | false | Execution Policy for set variable activity.|

#### SetVariable Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| variableName          | String          | true            | Name of the variable whose value needs to be set. |
| value                 | Object          | true            | Value to be set. Could be a static value or Expression. |
| setSystemVariable     | Boolean         | false           | If set to true, it sets the pipeline run return value. |

#### SecureInputOutputPolicy

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| secureInput           | Boolean         | false           | When set to true, Input from activity is considered as secure and will not be logged to monitoring.|
| secureOutput          | Object          | false           | When set to true, Output from activity is considered as secure and will not be logged to monitoring.|

### AppendVariable Activity Properties

Properties for activities with `type: "AppendVariable"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [AppendVariableActivityTypeProperties](#appendvariable-activity-type-properties) | true   | The properties for appendvariable activity |

#### AppendVariable Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| variableName          | String          | true            | Name of the variable whose value needs to be appended to. |
| value                 | Object          | true            | Value to be appended. Could be a static value or Expression. |

### Delete Activity Properties

Properties for activities with `type: "Delete"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [DeleteActivityTypeProperties](#delete-activity-type-properties) | true   | The properties for delete activity |

#### Delete Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| datasetSettings       | Object          | true            | Delete activity dataset settings. |
| recursive             | Boolean         | false           | If true, files or sub-folders under current folder path will be deleted recursively. Default is false. |
| maxConcurrentConnections | Integer       | false           | The max concurrent connections to connect data source at the same time. Minimum: 1. |
| enableLogging         | Boolean         | false           | Whether to record detailed logs of delete-activity execution. Default value is false. |
| logStorageSettings    | Object          | false           | Log storage settings customer need to provide when enableLogging is true. |
| storeSettings         | Object          | false           | Delete activity store settings. |

### AzureHDInsight Activity Properties

Properties for activities with `type: "AzureHDInsight"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [AzureHDInsightActivityTypeProperties](#azurehdinsight-activity-type-properties) | true   | The properties for AzureHDInsight activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|
| externalReferences    | [ExternalReferences](#external-references)| false | External references to connection.|

#### AzureHDInsight Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| hdiActivityType       | String          | true            | The sub type that specified one of five HDInsight activity types (Hive, Pig, MapReduce, Streaming, Spark).|
| arguments             | String[]        | false           | User specified arguments to HDInsight Activity.|
| getDebugInfo          | String          | false           | Debug info option. Valid values: "None", "Always", "Failure" |
| scriptSettings        | [HDInsightScriptSettings](#hdinsightscriptsettings) | false | HDInsight script settings |
| defines               | Object          | false           | Allows user to specify defines for the job request. Type: key value pairs (or Expression with resultType object) |
| variables             | Object[]        | false           | User specified arguments under variable namespace. Type: array of strings (or Expression with resultType array) |
| queryTimeout          | Integer         | false           | Query timeout value (in minutes). Effective when the HDInsight cluster is with ESP (Enterprise Security Package) |

#### HDInsightScriptSettings

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| scriptPath            | Object          | false           | Script path. Type: string (or Expression with resultType string) |
| externalReferences    | Object          | true            | External references to connection |

### SparkJobDefinition Activity Properties

Properties for activities with `type: "SparkJobDefinition"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [SparkJobDefinitionActivityTypeProperties](#sparkjobdefinition-activity-type-properties) | true   | The properties for SparkJobDefinition activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|

#### SparkJobDefinition Activity Type Properties

| Name                        | Type            | Required        | Description       |
|-----------------------------|-----------------|-----------------|-------------------|
| sparkJobDefinitionId        | String          | true            | Spark Job Definition id.|
| workspaceId                 | String          | true            | Workspace id.|
| executableFile              | String          | false           | Main definition file.|
| mainClass                   | String          | false           | Main class name if a jar file is set for executableFile.|
| additionalLibraryUris       | String          | false           | ADLS gen2 paths for reference files.|
| commandLineArguments        | String          | false           | Command line arguments.|
| defaultLakehouse            | [FabricArtifact](#fabricartifact) | false | Lakehouse reference which should be the default Lakehouse context |
| additionalLakehouses        | String          | false           | List of additional Lakehouse reference.|
| environmentId               | String          | false           | Environment artifact that should be used to customize the execution, the environment should come from the same workspace as the SJD artifact.|

#### FabricArtifact

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| workspaceId           | Object          | true            | Workspace id. Type: string (or Expression with resultType string) |
| artifactId            | Object          | true            | Artifact id. Type: string (or Expression with resultType string) |

### Script Activity Properties

Properties for activities with `type: "Script"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [ScriptActivityTypeProperties](#script-activity-type-properties) | true   | The properties for Script activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|

#### Script Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| database              | Object          | true            | Database against which script will be executed |
| scripts               | Object          | true            | Array of script blocks |
| logSettings           | Object          | false           | Log settings of script activity |
| scriptBlockExecutionTimeout | String   | false           | ScriptBlock execution timeout |
| connectionVersion     | String          | false           | Connection version |

### WebHook Activity Properties

Properties for activities with `type: "WebHook"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [WebHookActivityTypeProperties](#webhook-activity-type-properties) | true   | The properties for WebHook activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|
| externalReferences    | [ExternalReferences](#external-references)| false | External References to a connection.|

#### WebHook Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| method                | String          | true            | Rest API method for endpoint |
| relativeUrl           | String          | true            | Relative URL for the endpoint |
| timeout               | String          | false           | The timeout within which the webhook should be called back. If there is no value specified, it defaults to 10 minutes |
| headers               | String          | false           | The user specified headers that will be sent to the request |
| body                  | String          | false           | Request body for endpoint |
| reportStatusOnCallBack | Boolean        | false           | Report Status On CallBack |
| disableCertValidation | Boolean         | false           | When set to true, it ignores any TLS/SSL errors from the server-side. Default value: false |

### AzureFunction Activity Properties

Properties for activities with `type: "AzureFunction"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [AzureFunctionActivityTypeProperties](#azurefunction-activity-type-properties) | true   | The properties for AzureFunction activity |
| policy                | [ActivityPolicy](#activity-policy)| false | Execution Policy for the activity.|
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the Azure Function connection.|

#### AzureFunction Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| method                | Object          | true            | Rest API method for endpoint |
| functionName          | String          | true            | Name of the Function that the Azure Function Activity will call |
| headers               | String          | false           | The user specified headers that will be sent to the request |
| body                  | String          | false           | Request body for endpoint |
| functionSetId         | String (Guid)   | false           | Function set id |
| workspaceId           | String (Guid)   | false           | Workspace id |
| operationType         | String          | false           | The operation type of the activity |
| parameters            | String          | false           | Parameters for azure function activity |

### Custom Activity Properties

Properties for activities with `type: "Custom"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [CustomActivityTypeProperties](#custom-activity-type-properties) | true   | The properties for Custom activity |
| staging               | [CustomActivityStagingProperties](#custom-activity-staging-properties) | false | Custom activity staging properties |
| externalReferences    | [ExternalReferences](#external-references)| true | External references to connection |

#### Custom Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| command               | String          | true            | Command for custom activity |
| externalReferences    | [ExternalReferences](#external-references) | true | External references to connection |
| folderPath            | String          | false           | Folder path for resource files |
| extendedProperties    | Object          | false           | User defined property bag. There is no restriction on the keys or values that can be used. The user specified custom activity has the full responsibility to consume and interpret the content defined |
| retentionTimeInDays   | Double          | false           | The retention time for the files submitted for custom activity |
| autoUserSpecification | String          | false           | Elevation level and scope for the user, default is nonadmin task |

#### Custom Activity Staging Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| externalReferences    | [ExternalReferences](#external-references) | true | External references to connection |

### InvokeCopyJob Activity Properties

Properties for activities with `type: "InvokeCopyJob"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [InvokeCopyJobActivityTypeProperties](#invokecopyjob-activity-type-properties) | true   | The properties for invoke copy job activity |

#### InvokeCopyJob Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| copyJobId             | String (Guid)   | true            | Copy Job id |
| workspaceId           | String (Guid)   | true            | Workspace id |

### ExecuteSSISPackage Activity Properties

Properties for activities with `type: "ExecuteSSISPackage"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [ExecuteSSISPackageActivityTypeProperties](#executessispackage-activity-type-properties) | true   | The properties for execute SSIS package activity |

#### ExecuteSSISPackage Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| packageLocation       | SSISPackageLocation | true        | Location of the SSIS package to execute |
| packageName           | String          | true            | Name of the SSIS package |
| runtime               | IntegrationRuntimeReference | false | Integration Runtime to use for package execution |
| parameters            | ParameterSpecification | false   | Parameters to pass to the SSIS package |
| projectParameters     | ParameterSpecification | false   | Project-level parameters for the SSIS package |
| packageParameters     | ParameterSpecification | false   | Package-level parameters for the SSIS package |
| projectConnectionManagers | ConnectionManagerSpecification | false | Project connection managers for the SSIS package |
| packageConnectionManagers | ConnectionManagerSpecification | false | Package connection managers for the SSIS package |
| propertyOverrides     | PropertyOverrideSpecification | false | Property overrides for the SSIS package |
| logLocation           | SSISLogLocation | false           | Location to store execution logs |

### SqlServerStoredProcedure Activity Properties

Properties for activities with `type: "SqlServerStoredProcedure"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [SqlServerStoredProcedureActivityTypeProperties](#sqlserverstoredprocedure-activity-type-properties) | true   | The properties for SQL Server stored procedure activity |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection.|
| linkedService         | Object          | false           | Linked service |
| connectionSettings    | Object          | false           | Connection settings |

#### SqlServerStoredProcedure Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| database              | String          | false           | The database name of the SQL server |
| storedProcedureName   | String          | true            | Stored procedure name |
| storedProcedureParameters | Object      | false           | Value and type setting for stored procedure parameters |

### InvokePipeline Activity Properties

Properties for activities with `type: "InvokePipeline"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [InvokePipelineActivityTypeProperties](#invokepipeline-activity-type-properties) | true   | The properties for invoke pipeline activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the connection.|
| policy                | [ActivityPolicy](#activity-policy) | false | Activity policy.|

#### InvokePipeline Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| parameters            | Object          | false           | Pipeline parameters |
| waitOnCompletion      | Boolean         | false           | Defines whether activity execution will wait for the dependent pipeline execution to finish. Default is false |
| workspaceId           | String          | false           | Workspace ID |
| pipelineId            | String          | false           | Pipeline ID |
| operationType         | String          | false           | Operation type |

### KustoQueryLanguage Activity Properties

Properties for activities with `type: "KustoQueryLanguage"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [KustoQueryLanguageActivityTypeProperties](#kustoquerylanguage-activity-type-properties) | true   | The properties for Kusto query language activity |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection.|
| linkedService         | Object          | false           | Linked service |
| connectionSettings    | Object          | false           | Connection settings |

#### KustoQueryLanguage Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| command               | String          | true            | A control command, according to the KQL script activity command syntax |
| commandTimeout        | String          | false           | Control command timeout |
| database              | String          | false           | Database name to query |

### AzureML Activity Properties

Properties for activities with `type: "AzureMLExecutePipeline"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [AzureMLActivityTypeProperties](#azureml-activity-type-properties) | true   | The properties for Azure ML activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the Azure ML connection.|
| policy                | [ActivityPolicy](#activity-policy)| true    | Execution policy for the activity.|

#### AzureML Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| mlExecutionType       | String          | false           | Either pipeline or batch endpoints.|
| mlPipelineId          | String          | false          | ID of the published Azure ML pipeline. |
| mlPipelineEndpointId  | String          | false           | ID of the published Azure ML pipeline endpoint. |
| version               | String          | false           | Version of the published Azure ML pipeline endpoint.|
| experimentName        | String          | false           | Name of the Azure ML experiment |
| mlPipelineParameters  | ParameterSpecification | false     | Parameters to pass to the Azure ML pipeline |
| dataPathAssignments   | DataPathAssignment | false         | Data path assignments for the Azure ML pipeline |
| mlBatchEndpointName   | String          | false            | Name of the published Azure ML batch endpoint.|
| mlBatchDeploymentName | String          | false            | Name of the published Azure ML batch deployment for the selected endpoint.|
| jobSettings           | Object          | false            | Key,Value pairs to be passed to the published Azure ML batch endpoint.|
| jobInputs             | Object          | false            | Dictionary used for job input parameters.|
| jobOutputs            | Object          | false            | Dictionary used for job output parameters.|
| mlParentRunId         | String          | false            | The parent Azure ML Service pipeline run id.|
| continueOnStepFailure | Boolean         | false            | Whether to continue execution of other steps in the PipelineRun if a step fails.|

### DataLakeAnalyticsScope Activity Properties

Properties for activities with `type: "DataLakeAnalyticsScope"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [DataLakeAnalyticsScopeActivityTypeProperties](#datalakeanalyticsscope-activity-type-properties) | true   | The properties for Data Lake Analytics Scope activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the connection.|

#### DataLakeAnalyticsScope Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| scriptFileName        | String          | false           | Case-sensitive file name that has the Scope script |
| scriptFolderPath      | String          | false           | Case-sensitive path to folder that contains the Scope script |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection |
| linkedService         | Object          | false           | Script linked service |
| connectionSettings    | Object          | false           | Connection settings |
| degreeOfParallelism   | Integer         | false           | The maximum number of nodes simultaneously used to run the job. Default value is 1. Minimum: 1 |
| priority              | Integer         | false           | Determines which jobs out of all that are queued should be selected to run first. The lower the number, the higher the priority. Default value is 1000. Minimum: 1 |
| parameters            | Object          | false           | Parameters for Scope job request |
| runtimeVersion        | String          | false           | Runtime version of the Scope engine to use |
| jobName               | String          | false           | Azure Data Lake Analytics scope job name |
| jobOwner              | String          | false           | Custom tag to be added to scope job to indicate job owner alias |
| degreeOfParallelismPercent | Integer    | false           | The maximum percentage of nodes simultaneously used to run the job. Note that this and degreeofParallelism property are mutually exclusive |
| nebulaArguments       | String          | false           | Additional Scope parameters to pass in during job submission |
| notifier              | Object          | false           | List of email addresses to be notified when the job reaches a terminal state |
| scopeScriptInclusionSet | String        | false           | List of script resource file extensions separated by semicolons. Only these files will be uploaded to ADLA as scope job resources |
| tags                  | Object          | false           | Custom tags for Scope job |

### TridentNotebook Activity Properties

Properties for activities with `type: "TridentNotebook"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [TridentNotebookActivityTypeProperties](#tridentnotebook-activity-type-properties) | true   | The properties for Trident notebook activity |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection.|

#### TridentNotebook Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| notebookId            | String          | true            | Notebook ID |
| workspaceId           | String          | true            | Workspace ID |
| parameters            | Object          | false           | Parameters to be used for each run of this job. If the notebook takes a parameter that is not specified, the default value from the notebook will be used |
| sessionTag            | String          | false           | Spark session tag |

### DatabricksNotebook Activity Properties

Properties for activities with `type: "DatabricksNotebook"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [DatabricksNotebookActivityTypeProperties](#databricksnotebook-activity-type-properties) | true   | The properties for Databricks notebook activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the Databricks connection.|

#### DatabricksNotebook Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| notebookPath          | String          | true            | Path to the Databricks notebook to execute |
| baseParameters        | ParameterSpecification | false     | Base parameters to pass to the Databricks notebook |
| libraries             | DatabricksLibrarySpecification[] | false | Libraries to install for the notebook execution |
| existingClusterId     | String          | false           | ID of an existing Databricks cluster to use |
| newClusterSettings    | DatabricksNewClusterSettings | false | Settings for creating a new Databricks cluster |
| pythonWheelTask       | DatabricksPythonWheelTask | false   | Python wheel task configuration for Databricks |
| sparkJarTask          | DatabricksSparkJarTask | false     | Spark JAR task configuration for Databricks |
| sparkPythonTask       | DatabricksSparkPythonTask | false  | Spark Python task configuration for Databricks |

### RefreshDataFlow Activity Properties

Properties for activities with `type: "RefreshDataFlow"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [RefreshDataFlowActivityTypeProperties](#refreshdataflow-activity-type-properties) | true   | The properties for refresh data flow activity |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection.|
| policy                | [ActivityPolicy](#activity-policy) | false | Activity policy.|

#### RefreshDataFlow Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| dataflowId            | String          | true            | Dataflow ID |
| workspaceId           | String          | true            | Workspace ID |
| notifyOption          | String          | false           | Notification on mail settings |
| dataflowType          | String          | false           | The type of the dataflow (system property) |
| parameters            | Object          | false           | Parameters to be used for each run of this job. If the dataflow takes a parameter that is not specified, the default value from the dataflow will be used |

### Office365Email Activity Properties

Properties for activities with `type: "Office365Email"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [Office365EmailActivityTypeProperties](#office365email-activity-type-properties) | true   | The properties for Office 365 email activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the Office 365 connection.|

#### Office365Email Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| body                  | String          | false           | Content of the mail |
| to                    | String          | false           | List of recipients separated by ; |
| subject               | String          | false           | Subject of the mail |
| from                  | String          | false           | Sender email ID |
| cc                    | String          | false           | List of recipients in CC separated by ; |
| bcc                   | String          | false           | List of recipients in BCC separated by ; |
| sensitivity           | String          | false           | Sensitivity of the mail |
| importance            | String          | false           | Importance of the mail |
| replyTo               | String          | false           | Email ID where you want to receive the reply of the message |
| operationType         | String          | false           | Operation type |

### Email Activity Properties

Properties for activities with `type: "Email"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [EmailActivityTypeProperties](#email-activity-type-properties) | true   | The properties for email activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the email connection.|

#### Email Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| inputs                | [LogicAppsActivityInput](#logicappsactivityinput) | false | Logic Apps activity inputs |
| body                  | String          | false           | Content of the mail |
| to                    | String          | false           | List of recipients separated by ; |
| subject               | String          | false           | Subject of the mail |
| from                  | String          | false           | Sender email ID |
| cc                    | String          | false           | List of recipients in CC separated by ; |
| bcc                   | String          | false           | List of recipients in BCC separated by ; |
| sensitivity           | String          | false           | Sensitivity of the mail |
| importance            | String          | false           | Importance of the mail |
| replyTo               | String          | false           | Email ID where you want to receive the reply of the message |
| operationType         | String          | false           | Operation type |

### MicrosoftTeams Activity Properties

Properties for activities with `type: "MicrosoftTeams"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [MicrosoftTeamsActivityTypeProperties](#microsoftteams-activity-type-properties) | true   | The properties for Microsoft Teams activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the connection.|

#### MicrosoftTeams Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| content               | String          | false           | Content of the message |
| teamId                | String          | false           | Group ID or Workspace ID |
| channelId             | String          | false           | Model Dataset ID |
| chatId                | String          | false           | Type of refresh |
| operationType         | String          | false           | Operation type |
| subject               | String          | false           | Subject for channel posts |

### Teams Activity Properties

Properties for activities with `type: "Teams"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [TeamsActivityTypeProperties](#teams-activity-type-properties) | true   | The properties for Teams activity |
| externalReferences    | [ExternalReferences](#external-references)| false | Reference to the connection.|

#### Teams Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| inputs                | [LogicAppsActivityInput](#logicappsactivityinput) | false | Logic Apps activity inputs |
| content               | String          | false           | Content of the message |
| teamId                | String          | false           | Group ID or Workspace ID |
| channelId             | String          | false           | Model Dataset ID |
| chatId                | String          | false           | Type of refresh |
| operationType         | String          | false           | Operation type |

### PBISemanticModelRefresh Activity Properties

Properties for activities with `type: "PBISemanticModelRefresh"`.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| typeProperties        | [PBISemanticModelRefreshActivityTypeProperties](#pbisemanticmodelrefresh-activity-type-properties) | true   | The properties for Power BI semantic model refresh activity |
| externalReferences    | [ExternalReferences](#external-references)| true | Reference to the Power BI connection.|

#### PBISemanticModelRefresh Activity Type Properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| method                | String          | false           | HTTP method |
| groupId               | String          | false           | Group ID or Workspace ID |
| workspaceId           | String          | false           | Workspace ID |
| datasetId             | String          | false           | Model Dataset ID |
| type                  | String          | false           | Type of refresh |
| commitMode            | String          | false           | Refresh commit mode |
| maxParallelism        | Integer         | false           | Max parallelism |
| retryCount            | Integer         | false           | Retry count for refresh |
| objects               | Array           | false           | List of table partition objects |
| waitOnCompletion      | Boolean         | false           | Defines whether activity execution will wait for the dependent pipeline execution to finish. Default is false |
| inputs                | [LogicAppsActivityInput](#logicappsactivityinput) | false | Logic Apps activity inputs |
| operationType         | String          | false           | Operation type |

#### LogicAppsActivityInput

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| body                  | String          | true            | The body contains all the user-provided parameters |
| method                | String          | true            | The method of the Logic Apps activity operation |
| path                  | String          | true            | The swagger path identifying the Logic Apps activity operation |
| headers               | String          | false           | The headers for the Logic Apps activity operation |
| queries               | String          | false           | The queries for the Logic Apps activity operation |

### ContentDetails example

```json
{
    "properties": { 
        "description": "Data pipeline with multiple activity types demonstrating different typeProperties", 
        "activities": [
          {
            "name": "Notebook1",
            "type": "TridentNotebook",
            "dependsOn": [],
            "policy": {
              "timeout": "0.12:00:00",
              "retry": 0,
              "retryIntervalInSeconds": 30,
              "secureOutput": false,
              "secureInput": false
            },
            "typeProperties": {
              "notebookId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
              "workspaceId": "b81f1d9f-33c7-462d-b818-2e4906a123f3"
            }
          },
          {
            "name": "Get Metadata1",
            "type": "GetMetadata",
            "dependsOn": [],
            "policy": {
              "timeout": "0.12:00:00",
              "retry": 0,
              "retryIntervalInSeconds": 30,
              "secureOutput": false,
              "secureInput": false
            },
            "typeProperties": {
              "fieldList": [
                "columnCount"
              ],
              "datasetSettings": {
                "annotations": [],
                "linkedService": {
                  "name": "LakehouseGitArtifactW1",
                  "properties": {
                    "annotations": [],
                    "type": "Lakehouse",
                    "typeProperties": {
                      "workspaceId": "b81f1d9f-33c7-462d-b818-2e4906a123f3",
                      "artifactId": "c69ca3a7-fc70-4b4f-aad7-ce711b7a57d0",
                      "rootFolder": "Tables"
                    }
                  }
                },
                "type": "LakehouseTable",
                "schema": [],
                "typeProperties": {
                  "table": "Lh1"
                }
              }
            }
          },
          {
            "name": "Lookup1",
            "type": "Lookup",
            "dependsOn": [],
            "policy": {
              "timeout": "0.12:00:00",
              "retry": 0,
              "retryIntervalInSeconds": 30,
              "secureOutput": false,
              "secureInput": false
            },
            "typeProperties": {
              "source": {
                "type": "LakehouseTableSource"
              },
              "datasetSettings": {
                "annotations": [],
                "linkedService": {
                  "name": "LakehouseGitArtifactW1",
                  "properties": {
                    "annotations": [],
                    "type": "Lakehouse",
                    "typeProperties": {
                      "workspaceId": "b81f1d9f-33c7-462d-b818-2e4906a123f3",
                      "artifactId": "c69ca3a7-fc70-4b4f-aad7-ce711b7a57d0",
                      "rootFolder": "Tables"
                    }
                  }
                },
                "type": "LakehouseTable",
                "schema": [],
                "typeProperties": {
                  "table": "lh2"
                }
              }
            }
          },
          {
            "name": "PBISemanticModelRefresh1",
            "type": "PBISemanticModelRefresh",
            "dependsOn": [],
            "userProperties": [],
            "typeProperties": {
              "method": "POST",
              "groupId": "b81f1d9f-33c7-462d-b818-2e4906a123f3",
              "datasetId": "67cd5565-49a2-401d-907f-744cc68b16c8",
              "type": "Full",
              "commitMode": "transactional",
              "maxParallelism": 2,
              "retryCount": 1,
              "waitOnCompletion": true,
              "operationType": "RefreshDataset"
            },
            "externalReferences": {
              "connection": "276c4d2d-855a-4c20-adbe-23622ad82704"
            },
            "policy": {
              "timeout": "7.00:00:00",
              "retry": 0,
              "retryIntervalInSeconds": 30,
              "secureInput": false,
              "secureOutput": false
            }
          }
        ]
    } 
} 
```

Mandatory rules for deployment. You must implement every rule exactly as stated, with no exceptions or omissions, even if it is not a common pattern or seems redundant. Do not use your own judgment to simplify, skip, or modify any rule. If a rule is present, it must be enforced in the code, regardless of context. Adjust {{IacType}} files to align with these rules.

[IMPORTANT] Call quota tools first to get available regions and SKU quota for each resources. Resource region can be different with resource group, set each resource location accordingly!
[IMPORTANT] If current region for a resource is not available, 1)choose an available region and 2)add an index to the resource name suffix to avoid naming conflict. Don't change other resources' region in the resource group.

## Deployment Tool {{DeploymentTool}} rules:
{{DeploymentToolRules}}

## IaC Type: {{IacType}} rules:
{{IacTypeRules}}

## Resources: {{ResourceTypesDisplay}}
{{ResourceSpecificRules}}

{{FinalInstructions}}

## Tools needed: {{RequiredTools}}

{{AdditionalNotes}}

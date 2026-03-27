# Azure Well-Architected Framework: AI Agent Usage Guide

## Purpose
This guide enables AI cloud solution architect agents to systematically apply the Azure Well-Architected Framework when architecting new Azure workloads.

## What is the Azure Well-Architected Framework?
The Azure Well-Architected Framework is a design framework to improve the quality of Azure workloads. It consists of five pillars — **Reliability**, **Security**, **Cost Optimization**, **Operational Excellence**, and **Performance Efficiency** — that help ensure workloads are resilient, secure, cost-effective, well-operated, and performant.

## Building blocks of the framework

The framework consists of three hierarchical layers that **must be understood in sequence**:

### Layer 1: Pillars (Foundation)
Five pillars form the foundational layer and must be comprehended before proceeding to subsequent layers.

#### Five Core Pillars
1. **Reliability**: Resiliency, availability, and recovery
2. **Security**: Data protection, threat detection, and mitigation
3. **Cost Optimization**: Cost modeling, budgets, and waste reduction
4. **Operational Excellence**: Holistic observability and DevOps practices
5. **Performance Efficiency**: Scalability and load testing

#### Elements Within Each Pillar Guide
1. **Design Principles**: Foundation of good architectural design
   - **Goal**: Specific objective to achieve
   - **Approaches**: Recommended approaches to achieve the goal
2. **Design Strategies**: Methods to achieve pillar objectives
   - **Recommendations**: Actionable recommendations with Azure-specific guidance
   - **Azure Facilitations**: Azure services and features for implementation
3. **Cloud Design Patterns**: Industry-proven patterns addressing common challenges
4. **Tradeoffs**: Recognized compromises that balance competing priorities
5. **Maturity Model**: Phased adoption approach from early-stage to business-critical workloads

### Layer 2: Workloads (Application)
Workload-specific guidance on how pillars apply to specific workload classes (e.g., mission-critical, IoT, AI/ML).

#### Elements Within Each Workload Guide
1. **Get Started**: Common challenges and design areas for the workload class
2. **Design Principles**: Workload-specific application of pillar guidance
3. **Design Areas**: Technical decision points with prioritized recommendations

### Layer 3: Service Guides (Implementation)
Guidance on configuring individual Azure services within a workload.

#### Elements Within Each Service Guide
1. **Pillar Guides**: Service-specific application of each pillar
   - **Design Recommendations**: Service-specific recommendations beyond generic pillar guidance
   - **Configuration Recommendations**: Actionable service configurations
2. **Azure Policies**: Service-specific built-in policies supporting recommendations
3. **Example Architectures**: Reference architectures demonstrating recommendations

## Systematic Application Process

To apply the Azure Well-Architected Framework to a new Azure workload, follow this process in **strictly sequential order**. This ensures a principled, comprehensive approach to architectural decision-making.

---

## Fetch Pillar Guides

**⚠️ IMPORTANT**: Fetch all five pillar guides first, but do not read them yet. You will receive specific instructions on which sections to read at each phase.

### Pillar Guide Format
- Pillar guides are structured JSON documents with reference links to complete documentation
- Do not follow reference links until explicitly instructed
- You will be told which JSON fields to read at each step

### MCP Tool Calls to Fetch Guides
```bash
wellarchitectedframework_pillarguide_get --pillar reliability
wellarchitectedframework_pillarguide_get --pillar security
wellarchitectedframework_pillarguide_get --pillar cost
wellarchitectedframework_pillarguide_get --pillar operations
wellarchitectedframework_pillarguide_get --pillar performance
```

---

## Phase 1: Learning Phase

**Objective**: Build foundational understanding of all five pillars before making design decisions.

**Process**: Complete Step 1 for all pillars, then complete Step 2 for all pillars. Do not proceed to Phase 2 until both steps are complete. 

### Step 1: Understand All Design Principles
**Objective**: Establish foundational architectural principles before implementation.

**Action Items**:
1. Study design principles for all five pillars. Read the `designPrinciples` JSON field for each pillar.
2. Review all approaches within each principle. Review all `approaches` within each principle

**Completion Criteria**: You can articulate the goal and approaches for each design principle across all five pillars.

### Step 2: Understand All Design Strategies
**Objective**: Understand available strategies to achieve pillar objectives.

**Action Items**:
1. Review design strategies for all five pillars
2. Read the `designStrategies` JSON field for each pillar
3. Review all `recommendations` and `azureFacilitations` within each strategy

**Completion Criteria**: You understand the full range of strategies, recommendations, and Azure capabilities available for each pillar.

---

## Phase 2: Design Phase

**Objective**: Apply framework knowledge to make workload-specific architectural decisions.

### Step 1: Prioritize Pillars

**Objective**: Determine pillar importance based on workload requirements.

**Action Items**:
1. Analyze workload characteristics and business requirements
2. Assign priority ranking to each of the five pillars (1=highest, 5=lowest)
3. Document rationale for prioritization
4. Consider business impact, regulatory requirements, and user expectations

**Example**: A financial services application might prioritize: Security (1), Reliability (2), Operational Excellence (3), Cost Optimization (4), Performance Efficiency (5).

**Completion Criteria**: Clear priority ranking with documented justification.

### Step 2: Select Design Strategies

**Objective**: Identify and prioritize strategies applicable to this workload.

**Action Items**:
1. Review all design strategies from Phase 1
2. Filter strategies based on:
   - **Relevance**: Applies to this workload's characteristics
   - **Actionability**: Can be implemented by an AI agent during architecture design
   - **Practicality**: Excludes strategies requiring human decision-making or approval
3. Exclude theoretical or vague strategies without clear implementation paths
4. Assign priority to selected strategies based on pillar priorities from Step 1
5. Document selected strategies with implementation rationale

**Completion Criteria**: Curated list of applicable strategies prioritized by pillar importance and workload needs.

### Step 3: Make Strategic Design Tradeoffs

**Objective**: Balance competing priorities across pillars through explicit, justified tradeoffs.

**⚠️ CRITICAL**: Tradeoffs are essential to good architecture. Prioritization can and should favor one pillar over another when justified by workload requirements.

**Action Items**:
1. Read the `tradeoffs` JSON field for all five pillars
2. Identify conflicts between pillar recommendations (e.g., security vs. performance, cost vs. reliability)
3. Make explicit decisions favoring higher-priority pillars when conflicts arise
4. Document each tradeoff decision with:
   - **Conflict**: What recommendations conflict
   - **Decision**: Which pillar/strategy was favored
   - **Rationale**: Why this decision aligns with workload priorities
   - **Impact**: What compromise was accepted
5. Ensure tradeoffs align with Step 1 pillar prioritization

**Example**: Choosing multiple availability zones (Reliability) over single-zone deployment (Cost Optimization) for a business-critical application.

**Completion Criteria**: Documented tradeoff decisions for all identified conflicts, with clear rationale and accepted compromises.

---

## Phase 3: Implementation Phase

**Objective**: Configure selected Azure services according to pillar recommendations and tradeoff decisions.

**Prerequisites**: Azure service selection complete based on design strategies from Phase 2.

### Fetch Service Guides

**⚠️ IMPORTANT**: Fetch service guides for all selected Azure services, but do not read them yet. Specific instructions follow.

### Service Guide Format
- Service guides are structured JSON documents with reference links to complete documentation
- Do not follow reference links until explicitly instructed
- You will be told which JSON fields to read at each step

### MCP Tool Calls to Fetch Service Guides
```bash
# Step 1: Get list of available service guides
wellarchitectedframework_serviceguide_get

# Step 2: Fetch guide for each selected service
wellarchitectedframework_serviceguide_get --service <service-name>
```

Repeat Step 2 for each Azure service in your architecture. 

### Step 1: Apply Service-Specific Design Recommendations

**Objective**: Configure each Azure service according to pillar guidance and tradeoff decisions.

**Action Items** (for each service):
1. Read the `designRecommendations` JSON field for each pillar
2. Apply recommendations in priority order (highest-priority pillars first)
3. Respect tradeoff decisions from Phase 2, Step 3
4. Document why specific recommendations were applied or skipped

**Completion Criteria**: Design decisions documented for each service across all applicable pillars.

### Step 2: Apply Service-Specific Configurations

**Objective**: Implement actionable configurations that operationalize design recommendations.

**Action Items** (for each service):
1. Read the `configurationRecommendations` JSON field for each pillar
2. Apply configurations in priority order based on pillar prioritization
3. Ensure configurations address cross-cutting concerns (e.g., networking, monitoring, security)
4. Validate configurations don't conflict with tradeoff decisions
5. Document configuration choices and rationale

**Completion Criteria**: Concrete configurations defined for each service.

### Step 3: Apply Azure Policies

**Objective**: Enforce compliance through Azure Policy.

**Action Items** (for each service):
1. Read the `azurePolicies` JSON field
2. Identify applicable built-in policies for this workload
3. Apply policies that enforce pillar recommendations
4. Document policy assignments with rationale

**Completion Criteria**: Azure Policy assignments defined for governance and compliance.

---

## Next Steps

After completing all three phases:

1. **Review**: Validate that all decisions align with pillar priorities and tradeoffs
2. **Document**: Create architecture documentation summarizing:
   - Pillar prioritization and rationale
   - Selected design strategies
   - Tradeoff decisions and accepted compromises
   - Service configurations and policy assignments
3. **Validate**: Ensure architecture meets workload requirements

## Summary

By following this process sequentially, AI agents can make principled architectural decisions that balance competing priorities and align with Azure Well-Architected Framework best practices.

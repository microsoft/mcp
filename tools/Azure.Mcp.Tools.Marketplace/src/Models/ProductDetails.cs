// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Marketplace.Models;
// ===============================
// MAIN PRODUCT CLASSES
// ===============================

public class ProductDetails : ProductSummary
{
    public string? Language { get; set; }
    public bool? HasStandardContractAmendments { get; set; }
    public Guid? StandardContractAmendmentsRevisionId { get; set; }
    public string? UniversalAmendmentUrl { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsStopSell { get; set; }
    public StopSellInfo? StopSellInfo { get; set; }
    public MarketingMaterial? MarketingMaterial { get; set; }
    public string? LegalTermsUri { get; set; }
    public string? CSPLegalTermsUri { get; set; }
    public LegalTermsType? LegalTermsType { get; set; }
    public string? PrivacyPolicyUri { get; set; }
    public string? SupportUri { get; set; }
    public string? UiDefinitionUri { get; set; }
    public IList<string>? ScreenshotUris { get; set; }
    public IList<LinkProperties>? Links { get; set; }
    public IList<string>? LinkedAddIns { get; set; }
    public string? LargeIconUri { get; set; }
    public string? WideIconUri { get; set; }
    public IList<ImageGroup>? Images { get; set; }
    public IList<Artifact>? Artifacts { get; set; }
    public IList<ProductVideo>? Videos { get; set; }
    public IDictionary<string, string>? AdditionalProperties { get; set; }
    public string? PricingDetailsUri { get; set; }
    public bool? IsReseller { get; set; }
    public bool? DisableSendEmailOnPurchase { get; set; }
    public string? ProductCustomData { get; set; }
    public bool? HasAddOns { get; set; }
    public ExperienceConfig? ExperienceConfig { get; set; }
}

public class ProductSummary
{
    public string? DisplayName { get; set; }
    public double? Popularity { get; set; }
    public IReadOnlyList<string>? CategoryIds { get; set; }
    public IReadOnlyList<string>? IndustryIds { get; set; }
    public string? PublisherId { get; set; }
    public AzureBenefit? AzureBenefit { get; set; }
    public IReadOnlyList<string>? Badges { get; set; }
    public IReadOnlyList<string>? ProductBadges { get; set; }
    public IReadOnlyList<string>? ProductLabels { get; set; }
    public PublisherType? PublisherType { get; set; }
    public PublishingStage? PublishingStage { get; set; }
    public string? UniqueProductId { get; set; }
    public string? ProductType { get; set; }
    public IReadOnlyList<string>? OperatingSystems { get; set; }
    public IReadOnlyList<PricingType>? PricingTypes { get; set; }
    public string? PublisherDisplayName { get; set; }
    public string? LongSummary { get; set; }
    public string? Summary { get; set; }
    public IDictionary<string, string>? LinkedAddInsTypes { get; set; }
    public string? SmallIconUri { get; set; }
    public string? MediumIconUri { get; set; }
    public string? Description { get; set; }
    public IReadOnlyList<RatingBucket>? RatingBuckets { get; set; }
    public double? RatingAverage { get; set; }
    public int? RatingCount { get; set; }
    public MarketStartPrice? StartingPrice { get; set; }
    public IReadOnlyList<PlanSummary>? Plans { get; set; }
    public IReadOnlyList<string>? SupportedProducts { get; set; }
    public IReadOnlyList<string>? ApplicableProducts { get; set; }
    public DateTimeOffset? LastModifiedDateTime { get; set; }
    public bool? IsCoreVm { get; set; }
    public string? ProductOwnershipSellingMotion { get; set; }
    public string? ReservationResourceType { get; set; }
    public bool? IsTestProduct { get; set; }
    public string? OfferId { get; set; }
    public string? MixProductId { get; set; }
    public string? LicenseManagementType { get; set; }
    public string? OfferingType { get; set; }
    public LeadGenerationInfo? LeadGeneration { get; set; }
    [JsonPropertyName("hasRiPlans")]
    public bool? HasRIPlans { get; set; }
}

public class ProductsListResponse
{
    public List<ProductSummary>? Value { get; set; }
    public string? NextLink { get; set; }
}

// ===============================
// PLAN CLASSES (HIERARCHY ORDER)
// ===============================

public class PlanDetails : PlanSummary
{
    public string? Id { get; set; }
    public IList<Availability>? Availabilities { get; set; }
    public string? UiDefinitionUri { get; set; }
    public IList<Artifact>? Artifacts { get; set; }
    public string? GalleryItemVersion { get; set; }
    public bool? IsHidden { get; set; }
    public bool? IsStopSell { get; set; }
    public StopSellInfo? StopSellInfo { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public bool? IsQuantifiable { get; set; }
    public IList<BillingComponent>? BillingComponents { get; set; }
    public IList<PurchaseDurationDiscount>? PurchaseDurationDiscounts { get; set; }
    public bool? IsHiddenPrivateOffer { get; set; }
    public IList<LinkProperties>? Certifications { get; set; }
    public string? CustomerInstruction { get; set; }
    public string? PlanArtifactsVersion { get; set; }
    public IList<TermUpn>? Upns { get; set; }
    public string? SkuName { get; set; }
    public string? ManifestTenantId { get; set; }
    public string? ManifestVersion { get; set; }
}

public class PlanSummary
{
    public string? PlanId { get; set; }
    public string? UniquePlanId { get; set; }
    public string? DisplayName { get; set; }
    public VmArchitectureType? VmArchitectureType { get; set; }
    public CspState? CspState { get; set; }
    public PlanMetadata? Metadata { get; set; }
    public string? AltStackReference { get; set; }
    public string? StackType { get; set; }
    public string? AltArchitectureReference { get; set; }
    public IReadOnlyList<string>? CategoryIds { get; set; }
    public bool? HasProtectedArtifacts { get; set; }
    public IReadOnlyList<PricingType>? PricingTypes { get; set; }
    public IReadOnlyList<VmSecurityType>? VmSecurityTypes { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? SkuId { get; set; }
    public string? Type { get; set; }
    public string? DisplayRank { get; set; }
    public bool? IsPrivate { get; set; }
    public IReadOnlyList<string>? PlanLabels { get; set; }
    public LeadGenerationInfo? LeadGeneration { get; set; }
    [JsonPropertyName("hasRi")]
    public bool? HasRI { get; set; }
}

// ===============================
// AVAILABILITY AND PRICING CLASSES
// ===============================

public class Availability
{
    public string? Id { get; set; }
    public IList<string>? Actions { get; set; }
    public Meter? Meter { get; set; }
    public string? PricingAudience { get; set; }
    public IList<Term>? Terms { get; set; }
    public bool? HasFreeTrials { get; set; }
    public string? ConsumptionUnitType { get; set; }
    public int? DisplayRank { get; set; }
    public InvoicingPolicy? InvoicingPolicy { get; set; }
}

public class Meter
{
    public string? Id { get; set; }
    public string? PartNumber { get; set; }
    public string? ConsumptionResourceId { get; set; }
    public Price? Price { get; set; }
    public string? MeterType { get; set; }
    public IList<IncludedQuantityProperty>? IncludedQuantityProperties { get; set; }
}

public class Term
{
    public List<TermDescriptionParameter>? TermDescriptionParameters { get; set; }
    public string? TermId { get; set; }
    public string? TermUnits { get; set; }
    public ProrationPolicy? ProrationPolicy { get; set; }
    public string? TermDescription { get; set; }
    public Price? Price { get; set; }
    public string? RenewTermId { get; set; }
    public string? RenewTermUnits { get; set; }
    public BillingPlan? BillingPlan { get; set; }
    public string? RenewToTermBillingPlan { get; set; }
    public bool? IsAutorenewable { get; set; }
}

public class Price
{
    public string? CurrencyCode { get; set; }
    public bool? IsPIRequired { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? MSRP { get; set; }
}

public class BillingPlan
{
    public string? BillingPeriod { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Price? Price { get; set; }
}

public class MarketStartPrice
{
    public string? Market { get; set; }
    public string? TermUnits { get; set; }
    public string? MeterUnits { get; set; }
    public decimal? MinTermPrice { get; set; }
    public decimal? MinMeterPrice { get; set; }
    public string? Currency { get; set; }
}

// ===============================
// METADATA AND SUPPORTING CLASSES
// ===============================

public class StopSellInfo
{
    public DateTime? StartDate { get; set; }
    public StopSellReason? Reason { get; set; }
    public string? AlternativeOfferId { get; set; }
    public string? AlternativePlanId { get; set; }
}

public class MarketingMaterial
{
    public string? Path { get; set; }
    public string? LearnUri { get; set; }
}

public class LinkProperties
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Uri { get; set; }
}

public class PlanMetadata
{
    public string? Generation { get; set; }
    public string? AltStackReference { get; set; }
    public IReadOnlyList<PlanSkuRelation>? RelatedSkus { get; set; }
}

public class PlanSkuRelation
{
    public RelatedSku? Sku { get; set; }
    public string? RelationType { get; set; }
}

public class RelatedSku
{
    public string? Name { get; set; }
    public string? Generation { get; set; }
    public string? Identity { get; set; }
}

// ===============================
// MEDIA AND CONTENT CLASSES
// ===============================

public class ImageGroup
{
    public ImageGroup()
    {
        Items = new List<Image>();
    }

    public string? Context { get; set; }
    public IList<Image>? Items { get; set; }
}

public class Image
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? ImageType { get; set; }
}

public class ProductVideo
{
    public string? Caption { get; set; }
    public Uri? Uri { get; set; }
    public string? VideoPurpose { get; set; }
    public PreviewImage? PreviewImage { get; set; }
}

public class PreviewImage
{
    public string? Caption { get; set; }
    public string? Uri { get; set; }
    public string? ImagePurpose { get; set; }
}

public class Artifact
{
    public string? Name { get; set; }
    public string? Uri { get; set; }
    public ArtifactType? Type { get; set; }
}

// ===============================
// BILLING AND POLICY CLASSES
// ===============================

public class BillingComponent
{
    public string? BillingTag { get; set; }
    public Dictionary<string, int[]>? CustomMeterIds { get; set; }
}

public class PurchaseDurationDiscount
{
    public string? Duration { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class InvoicingPolicy
{
    public string? InvoicingCadence { get; set; }
    public IList<FilterInstruction>? FilterInstructions { get; set; }
}

public class FilterInstruction
{
    public string? FilterName { get; set; }
    public string? Operator { get; set; }
    public IList<string>? Values { get; set; }
}

public class ProrationPolicy
{
    public string? MinimumProratedUnits { get; set; }
}

public class TermDescriptionParameter
{
    public string? Parameter { get; set; }
    public string? Value { get; set; }
}

public class IncludedQuantityProperty
{
    public string? TermId { get; set; }
    public string? Quantity { get; set; }
}

public class TermUpn
{
    public string? TermId { get; set; }
    public string? Upn { get; set; }
}

// ===============================
// ENUMS
// ===============================

public enum LegalTermsType
{
    None = 0,
    EA = 1
}

public enum ArtifactType
{
    Template,
    Fragment,
    Custom,
    Metadata
}

public enum AzureBenefit
{
    Eligible,
    NotEligible
}

public enum PublisherType
{
    Microsoft,
    ThirdParty
}

public enum PublishingStage
{
    Preview,
    Public
}

public enum PricingType
{
    Free = 0,
    FreeTrial = 1,
    Byol = 2,
    Payg = 3,
    Ri = 4,
    ContactPublisher = 5
}

public enum RatingBucket
{
    AboveOne = 1,
    AboveTwo = 2,
    AboveThree = 3,
    AboveFour = 4
}

public enum VmArchitectureType
{
    X64Gen1 = 0,
    X64Gen2 = 1,
    Arm64 = 2
}

public enum CspState
{
    OptIn,
    OptOut,
    Terminated,
    SelectiveOptIn
}

public enum VmSecurityType
{
    None = 0,
    Trusted = 1,
    Confidential = 2
}

public enum StopSellReason
{
    EndOfSupport,
    SecurityIssue,
    Other
}


public class LeadGenerationInfo
{
    public string? LeadGenId { get; set; }
    public bool? LeadGenEnabled { get; set; }
}

public class ExperienceConfig
{
    public AzurePortalConfig? AzurePortal { get; set; }

    public class AzurePortalConfig
    {
        public string? ExtensionName { get; set; }
        public string? AssetType { get; set; }
        public string? AssetKind { get; set; }
    }
}

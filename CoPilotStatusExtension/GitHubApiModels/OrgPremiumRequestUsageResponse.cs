
using System.Collections.Generic;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class PremiumRequestUsageResult
{
	public int TotalQuantity { get; set; }
	public int IncludedQuantity { get; set; }
	public int OverageQuantity { get; set; }
	public decimal TotalNetAmount { get; set; }
	public string? ErrorMessage { get; set; }
	public List<PremiumRequestUsageItem> Items { get; set; } = new();
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class OrgPremiumRequestUsageResponse
{
	[JsonProperty("usageItems")]
	public List<PremiumRequestUsageItem>? UsageItems { get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class PremiumRequestUsageItem
{
	[JsonProperty("product")]
	public string? Product { get; set; }

	[JsonProperty("sku")]
	public string? Sku { get; set; }

	[JsonProperty("model")]
	public string? Model { get; set; }

	[JsonProperty("unitType")]
	public string? UnitType { get; set; }

	[JsonProperty("pricePerUnit")]
	public decimal? PricePerUnit { get; set; }

	[JsonProperty("grossQuantity")]
	public int? GrossQuantity { get; set; }

	[JsonProperty("grossAmount")]
	public decimal? GrossAmount { get; set; }

	[JsonProperty("discountQuantity")]
	public int? DiscountQuantity { get; set; }

	[JsonProperty("discountAmount")]
	public decimal? DiscountAmount { get; set; }

	[JsonProperty("netQuantity")]
	public int? NetQuantity { get; set; }

	[JsonProperty("netAmount")]
	public decimal? NetAmount { get; set; }
}
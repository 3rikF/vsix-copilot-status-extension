using System.Collections.Generic;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class UserBillingUsageResponse
{
	[JsonProperty("usageItems")]
	public List<UserBillingUsageItem>? UsageItems { get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class UserBillingUsageItem
{
	[JsonProperty("product")]
	public string? Product { get; set; }

	[JsonProperty("pricePerUnit")]
	public double PricePerUnit { get; set; }

	[JsonProperty("quantity")]
	public double Quantity { get; set; }

	[JsonProperty("discountAmount")]
	public double DiscountAmount { get; set; }

	[JsonProperty("netAmount")]
	public double NetAmount { get; set; }
}

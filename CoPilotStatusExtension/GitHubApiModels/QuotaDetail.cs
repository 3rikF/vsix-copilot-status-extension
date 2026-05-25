
using System;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class QuotaDetail
{
	[JsonProperty("overage_count")]
	public int OverageCount { get; set; }

	[JsonProperty("overage_permitted")]
	public bool OveragePermitted { get; set; }

	[JsonProperty("percent_remaining")]
	public double PercentRemaining { get; set; }

	[JsonProperty("quota_id")]
	public string QuotaId { get; set; }

	[JsonProperty("quota_remaining")]
	public double QuotaRemaining { get; set; }

	[JsonProperty("unlimited")]
	public bool Unlimited { get; set; }

	[JsonProperty("timestamp_utc")]
	public DateTimeOffset TimestampUtc { get; set; }

	[JsonProperty("has_quota")]
	public bool HasQuota { get; set; }

	[JsonProperty("quota_reset_at")]
	public long QuotaResetAt { get; set; }

	[JsonProperty("token_based_billing")]
	public bool TokenBasedBilling { get; set; }

	[JsonProperty("remaining")]
	public int Remaining { get; set; }

	[JsonProperty("entitlement")]
	public int Entitlement { get; set; }
}
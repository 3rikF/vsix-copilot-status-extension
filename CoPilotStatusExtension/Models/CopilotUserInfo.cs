
// ignore spelling: mcp jetbrains

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed record class CopilotUserInfo
{
	//--- CoPilot Stats -----------------------------------------------------------------------
	public string Status						{ get; set; } = string.Empty; // OK, NotSignedInToGitHub
	public bool? ChatEnabled					{ get; set; }
	public bool? CompletionsEnabled				{ get; set; }
	public bool? EditorPreviewFeaturesEnabled	{ get; set; }
	public bool? McpEnabledByToken				{ get; set; }

	//--- TokenEnvelope ---
	public bool? AnnotationsEnabled				{ get; set; }

	//public bool? ChatEnabled					{ get; set; } ... again ...
	public bool? ChatJetbrainsEnabled			{ get; set; }
	public bool? CodeQuoteEnabled				{ get; set; }

	public bool? CopilotExclusionEnabled		{ get; set; }
	public bool? CopilotExclusion				{ get; set; }

	public object? ErrorDetails					{ get; set; }
	//public DateTime ExpiresAt					{ get; set; }

	//--- GitHub Stats ------------------------------------------------------------------------
	public bool? IsIndividual					{ get; set; }
	public bool? IsEnterprise					{ get; set; }
	public string Username						{ get; set; } = string.Empty;
	public string AccessToken					{ get; set; } = string.Empty;	// aka GitHubToken
	public string SubscriptionType				{ get; set; } = string.Empty;	//

	//--- Billing Usage (from GitHub API) -----------------------------------------------------
	//public CopilotChatStatistics? PersonalMetrics			{ get; set; }
	//public OrgCopilotMetricsResult EnterpriseMetrics		{ get; internal set; }
	//public PremiumRequestUsageResult OrganizationMetrics	{ get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CopilotBillingUsage
{
	/// <summary>Sum of netAmount across all Copilot billing items (actual charges in USD).</summary>
	public double TotalNetAmount		{ get; set; }

	/// <summary>Total number of Copilot requests this billing period.</summary>
	public double TotalQuantity			{ get; set; }

	/// <summary>Requests covered by the included quota (derived from discountAmount / pricePerUnit).</summary>
	public double TotalIncludedQuantity	{ get; set; }

	/// <summary>Requests beyond the included quota (= TotalQuantity - TotalIncludedQuantity).</summary>
	public double TotalOverageQuantity	{ get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
//public sealed class CopilotChatStatistics
//{
//	public string?			ErrorMessage		{ get; set; }
//	public QuotaSnapshots?	QuotaSnapshots		{ get; set; }
//	public string?			CopilotPlan			{ get; set; }
//	public string?			QuotaResetDate		{ get; set; }
//	public DateTimeOffset?	QuotaResetDateUtc	{ get; set; }
//}
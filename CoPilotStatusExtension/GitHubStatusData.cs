
using System;

using Microsoft.VisualStudio.OLE.Interop;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed record class GitHubStatusData
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
	public DateTime ExpiresAt					{ get; set; }

	//--- GitHub Stats ------------------------------------------------------------------------
	public bool? IsIndividual					{ get; set; }
	public bool? IsEnterprise					{ get; set; }
	public string GitHubUsername				{ get; set; } = string.Empty;
	public string GitHubPassword				{ get; set; } = string.Empty;	// aka GitHubToken
	public string SubscriptionType				{ get; set; } = string.Empty;	// yearly_subscriber_quota, ...
	public string[] OrganizationList			{ get; set; } = [];
	public string[] EnterpriseList				{ get; set; } = [];

	//public int		CoreRemaining			{ get; set; }
	//public int		CoreLimit				{ get; set; }
	//public int		SearchRemaining			{ get; set; }
	//public int		SearchLimit				{ get; set; }
	public string?	ErrorMessage				{ get; set; }

	//--- Billing Usage (from GitHub API) -----------------------------------------------------
	public CopilotBillingUsage? BillingUsage	{ get; set; }
	public CopilotChatStatistics? ChatStatistics { get; set; }

	//--- additional info in AuthInfo:
	// AuthInfo
	// {
	//		TokenEnvelope	= CopilotAuthToken
	//		{
	//			AdditionalProperties = System.Collections.Generic.Dictionary`2[System.String,System.Object],
	//			Token			= tid=47f573abe0618bc5f3464ce44d0c9e4d;exp=1779643582;iat=1779641782;sku=yearly_subscriber_quota;proxy-ep=proxy.individual.githubcopilot.com;st=dotcom;chat=1;cit=1;malfil=1;editor_preview_features=1;agent_mode=1;agent_mode_auto_approval=1;mcp=1;blackbird_external_indexing=1;client_byok=0;ccr=1;ip=91.20.209.170;asn=AS3320:db4523398990f26c409253050cd3125a21657e209896fa03c842d405c58c3326,
	//			RefreshIn		= 1500,
	//			VscPanelV2		= ,
	//			CopilotIdeAgentChatGpt4SmallPrompt = ,
	//			Sku				= yearly_subscriber_quota,
	//			Endpoints		= Endpoints
	//			{
	//				AdditionalProperties	= ,
	//				Proxy					= https://proxy.individual.githubcopilot.com,
	//				Api						= https://api.individual.githubcopilot.com,
	//				Telemetry				= https://telemetry.individual.githubcopilot.com,
	//				OriginTracker			= https://origin-tracker.individual.githubcopilot.com
	//			},
	//			GitHubTrackingId		= 47f573abe0618bc5f3464ce44d0c9e4d,
	//			LimitedUserQuotas		= ,
	//			LimitedUserResetDate	= ,
	//			CanSignupForLimited		= ,
	//			ErrorDetails			=
	//		},
	//		TokenEndpoint	= https://api.github.com/copilot_internal/v2/token,
	//		TelemetryStamp	= dotcom,
	//		Host			= https://github.com/,
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

	/// <summary>Non-null when the API call failed.</summary>
	public string? ErrorMessage			{ get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CopilotChatStatistics
{
	public string?			ErrorMessage		{ get; set; }
	public QuotaSnapshots?	QuotaSnapshots		{ get; set; }
	public string?			CopilotPlan			{ get; set; }
	public string?			QuotaResetDate		{ get; set; }
	public DateTimeOffset?	QuotaResetDateUtc	{ get; set; }
}
using System;

using Newtonsoft.Json;

public sealed class CopilotQuotaResponse
{
	[JsonProperty("login")]
	public string Login { get; set; }

	[JsonProperty("access_type_sku")]
	public string AccessTypeSku { get; set; }

	[JsonProperty("analytics_tracking_id")]
	public string AnalyticsTrackingId { get; set; }

	[JsonProperty("assigned_date")]
	public DateTimeOffset AssignedDate { get; set; }

	[JsonProperty("can_signup_for_limited")]
	public bool CanSignupForLimited { get; set; }

	[JsonProperty("chat_enabled")]
	public bool ChatEnabled { get; set; }

	[JsonProperty("cli_enabled")]
	public bool CliEnabled { get; set; }

	[JsonProperty("copilotignore_enabled")]
	public bool CopilotIgnoreEnabled { get; set; }

	[JsonProperty("copilot_plan")]
	public string CopilotPlan { get; set; }

	[JsonProperty("editor_preview_features_enabled")]
	public bool EditorPreviewFeaturesEnabled { get; set; }

	[JsonProperty("is_mcp_enabled")]
	public bool IsMcpEnabled { get; set; }

	[JsonProperty("organization_login_list")]
	public string[] OrganizationLoginList { get; set; }

	[JsonProperty("organization_list")]
	public string[] OrganizationList { get; set; }

	[JsonProperty("restricted_telemetry")]
	public bool RestrictedTelemetry { get; set; }

	[JsonProperty("cloud_session_storage_enabled")]
	public bool CloudSessionStorageEnabled { get; set; }

	[JsonProperty("cli_remote_control_enabled")]
	public bool CliRemoteControlEnabled { get; set; }

	[JsonProperty("endpoints")]
	public CopilotEndpoints Endpoints { get; set; }

	[JsonProperty("can_upgrade_plan")]
	public bool CanUpgradePlan { get; set; }

	[JsonProperty("quota_reset_date")]
	public string QuotaResetDate { get; set; }

	[JsonProperty("quota_snapshots")]
	public QuotaSnapshots QuotaSnapshots { get; set; }

	[JsonProperty("quota_reset_date_utc")]
	public DateTimeOffset QuotaResetDateUtc { get; set; }
}

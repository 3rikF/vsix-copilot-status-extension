
using System.Text;
using System.Windows.Controls;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views;

//-----------------------------------------------------------------------------------------------------------------------------------------
public partial class GitHubStatusBarControl : UserControl
{
	private GitHubStatusData? _statusData;

	public GitHubStatusBarControl()
		=> InitializeComponent();

	public string StatusText
	{
		get => StatusTextBlock.Text;
		set => StatusTextBlock.Text = value;
	}

	public string StatusTooltip
	{
		get => StatusTextBlock.ToolTip?.ToString() ?? string.Empty;
		set => StatusTextBlock.ToolTip = value;
	}

	public GitHubStatusData? StatusData
	{
		get => _statusData;
		set
		{
			_statusData		= value;
			StatusText		= GetStatusText(_statusData);
			StatusTooltip	= GetStatusTooltip(_statusData);
		}
	}

	private static string GetStatusText(GitHubStatusData? data)
	{
		if (data is null )
			return "Unknown Status";

		else if (data.Status == "NotSignedInToGitHub")
			return "GitHub Copilot: Not signed in";

		else if (data.Status == "OK")
		{


			if (data.PersonalMetrics?.QuotaSnapshots?.PremiumInteractions?.PercentRemaining is not null)
				return $"{data.GitHubUsername}: {data.PersonalMetrics.QuotaSnapshots.PremiumInteractions.PercentRemaining:F1}%";
			else
				return $"{data.GitHubUsername}: ...%";
		}

		else
			return $"Unhandled Status [{data.Status}]";
	}

	private static string GetStatusTooltip(GitHubStatusData? data)
	{
		if (data is null)
			return "GitHub Copilot status is not available.";

		StringBuilder sb = new StringBuilder()
			.AppendLine($"Status:			{data.Status}")
			.AppendLine($"User:			{data.GitHubUsername}")
			.AppendLine($"Subscription:		{data.SubscriptionType}")
			.AppendLine($"Account type:		{(data.IsEnterprise == true ? "Enterprise" : data.IsIndividual == true ? "Individual" : "Unknown")}")
			.AppendLine($"Organizations:		{(data.OrganizationList != null ? string.Join(", ", data.OrganizationList) : "None")}")
			.AppendLine($"Enterprises:		{(data.EnterpriseList != null ? string.Join(", ", data.EnterpriseList) : "None")}")
			.AppendLine()
			.AppendLine($"Chat:			{FormatBool(data.ChatEnabled)}")
			.AppendLine($"Completions:		{FormatBool(data.CompletionsEnabled)}")
			.AppendLine($"Annotations:		{FormatBool(data.AnnotationsEnabled)}")
			.AppendLine($"MCP:			{FormatBool(data.McpEnabledByToken)}")
			.AppendLine($"Editor preview:		{FormatBool(data.EditorPreviewFeaturesEnabled)}")
			.AppendLine($"Code quote:		{FormatBool(data.CodeQuoteEnabled)}")
			.AppendLine($"Chat JetBrains:		{FormatBool(data.ChatJetbrainsEnabled)}")
			.AppendLine($"Copilot exclusion A:	{FormatBool(data.CopilotExclusion)}")
			.AppendLine($"Copilot exclusion B:	{FormatBool(data.CopilotExclusionEnabled)}");

		if (data.OrganizationList?.Length > 0)
			_ = sb.AppendLine($"Organizations:		{string.Join(", ", data.OrganizationList)}");

		if (data.EnterpriseList?.Length > 0)
			_ = sb.AppendLine($"Enterprises:		{string.Join(", ", data.EnterpriseList)}");

		//--- Personal Metrics ------------------------------------------------
		if (data.PersonalMetrics is not null)
		{
			_ = sb
				.AppendLine()
				.AppendLine("Personal Metrics:");

			if (data.PersonalMetrics.ErrorMessage is not null)
				_ = sb.AppendLine($"  Error: [{data.PersonalMetrics.ErrorMessage}]");

			//--- premium quota ---------------------------
			if (data.PersonalMetrics.QuotaSnapshots?.PremiumInteractions is not null)
				_ = sb.Append(GetQuotaDetailToolTip("Premium Interactions", data.PersonalMetrics.QuotaSnapshots.PremiumInteractions));
			else
				_ = sb.AppendLine($"  Premium Interactions: [No data]");

			//--- Chat Interactions -----------------------
			if (data.PersonalMetrics.QuotaSnapshots?.Chat is not null)
				_ = sb.Append(GetQuotaDetailToolTip("Chat Interactions", data.PersonalMetrics.QuotaSnapshots.Chat));
			else
				_ = sb.AppendLine($"  Chat Interactions: [No data]");

			//--- Completions -----------------------------
			if (data.PersonalMetrics.QuotaSnapshots?.Completions is not null)
				_ = sb.Append(GetQuotaDetailToolTip("Completions", data.PersonalMetrics.QuotaSnapshots.Completions));
			else
				_ = sb.AppendLine($"  Completions: [No data]");
		}

		//--- Organization Metrics --------------------------------------------
		if (data.OrganizationMetrics is not null)
		{
			_ = sb.AppendLine().AppendLine("Organization Metrics:");
			if (data.OrganizationMetrics.ErrorMessage is not null)
				_ = sb.AppendLine($"  Error: [{data.OrganizationMetrics.ErrorMessage}]");

			//--- premium quota ---------------------------
		}

		//--- return result ---------------------------------------------------
		return sb.ToString().TrimEnd();
	}

	private static StringBuilder GetQuotaDetailToolTip(string title, QuotaDetail detail)
	{
		if (detail.Unlimited)
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"  Unlimited");
		}
		else
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"  Remaining:	{detail.PercentRemaining:F1}%  ({detail.QuotaRemaining} / {detail.Entitlement})")
				.AppendLine($"  Overage:	{detail.OverageCount} (permitted: {FormatBool(detail.OveragePermitted)})");
		}
	}

	private static string FormatBool(bool? value) => value switch
	{
		true  => "✔",
		false => "✘",
		null  => "❓",
	};
}

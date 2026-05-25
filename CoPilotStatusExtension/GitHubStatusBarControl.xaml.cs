using System.Text;
using System.Windows.Controls;

namespace CoPilotStatusExtension;

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


			if (data.ChatStatistics?.QuotaSnapshots?.PremiumInteractions?.PercentRemaining is not null)
				return $"{data.GitHubUsername}: {data.ChatStatistics.QuotaSnapshots.PremiumInteractions.PercentRemaining:F1}%";
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

		if (data.Status != "OK")
			return $"Error: {data.ErrorMessage ?? "Unknown error"}";

		StringBuilder sb = new StringBuilder()
			.AppendLine($"User:			{data.GitHubUsername}")
			.AppendLine($"Subscription:		{data.SubscriptionType}")
			.AppendLine($"Account type:		{(data.IsEnterprise == true ? "Enterprise" : data.IsIndividual == true ? "Individual" : "Unknown")}")
			.AppendLine()
			.AppendLine($"Chat:			{FormatBool(data.ChatEnabled)}")
			.AppendLine($"Completions:		{FormatBool(data.CompletionsEnabled)}")
			.AppendLine($"Annotations:		{FormatBool(data.AnnotationsEnabled)}")
			.AppendLine($"MCP:			{FormatBool(data.McpEnabledByToken)}")
			.AppendLine($"Editor preview:		{FormatBool(data.EditorPreviewFeaturesEnabled)}")
			.AppendLine($"Code quote:		{FormatBool(data.CodeQuoteEnabled)}")
			.AppendLine($"Chat JetBrains:		{FormatBool(data.ChatJetbrainsEnabled)}")
			.AppendLine($"Copilot exclusion:		{FormatBool(data.CopilotExclusion)}");

		if (data.OrganizationList?.Length > 0)
			_ = sb.AppendLine($"Organizations:		{string.Join(", ", data.OrganizationList)}");

		if (data.EnterpriseList?.Length > 0)
			_ = sb.AppendLine($"Enterprises:		{string.Join(", ", data.EnterpriseList)}");

		return sb.ToString().TrimEnd();
	}

	private static string FormatBool(bool? value) => value switch
	{
		true  => "✔",
		false => "✘",
		null  => "❓",
	};
}

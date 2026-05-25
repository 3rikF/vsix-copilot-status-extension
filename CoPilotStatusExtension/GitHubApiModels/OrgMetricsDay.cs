using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

public sealed class OrgMetricsDay
{
	[JsonProperty("date")]
	public string? Date { get; set; }

	[JsonProperty("total_active_users")]
	public int? TotalActiveUsers { get; set; }

	[JsonProperty("total_engaged_users")]
	public int? TotalEngagedUsers { get; set; }

	[JsonProperty("copilot_ide_code_completions")]
	public OrgCodeCompletionsMetrics? CopilotIdeCodeCompletions { get; set; }
}

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

public sealed class OrgLanguageMetrics
{
	[JsonProperty("total_code_suggestions")]
	public int? TotalCodeSuggestions { get; set; }
}

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class OrgCopilotMetricsResult
{
	public int Days { get; set; }
	public string? Since { get; set; }
	public string? Until { get; set; }
	public int EngagedUsersSum { get; set; }
	public int CodeSuggestionsSum { get; set; }
	public string? ErrorMessage { get; set; }
}

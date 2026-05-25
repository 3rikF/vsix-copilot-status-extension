
using System.Collections.Generic;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

public sealed class OrgCodeCompletionsMetrics
{
	[JsonProperty("editors")]
	public List<OrgEditorMetrics>? Editors { get; set; }
}

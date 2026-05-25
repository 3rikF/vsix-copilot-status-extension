
using System.Collections.Generic;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

public sealed class OrgEditorMetrics
{
	[JsonProperty("models")]
	public List<OrgModelMetrics>? Models { get; set; }
}

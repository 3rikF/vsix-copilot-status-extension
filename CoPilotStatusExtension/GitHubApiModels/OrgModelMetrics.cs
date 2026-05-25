
using System.Collections.Generic;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

public sealed class OrgModelMetrics
{
	[JsonProperty("languages")]
	public List<OrgLanguageMetrics>? Languages { get; set; }
}


using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CopilotEndpoints
{
	[JsonProperty("api")]
	public string Api { get; set; }

	[JsonProperty("origin-tracker")]
	public string OriginTracker { get; set; }

	[JsonProperty("proxy")]
	public string Proxy { get; set; }

	[JsonProperty("telemetry")]
	public string Telemetry { get; set; }
}

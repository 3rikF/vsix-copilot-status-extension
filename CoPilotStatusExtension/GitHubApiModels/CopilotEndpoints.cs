
using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CopilotEndpoints
{
	[JsonProperty("api")]
	public string Api { get; set; } = string.Empty;

	[JsonProperty("origin-tracker")]
	public string OriginTracker { get; set; } = string.Empty;

	[JsonProperty("proxy")]
	public string Proxy { get; set; } = string.Empty;

	[JsonProperty("telemetry")]
	public string Telemetry { get; set; } = string.Empty;
}

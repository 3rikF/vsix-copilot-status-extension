
using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class QuotaSnapshots
{
	[JsonProperty("chat")]
	public QuotaDetail Chat { get; set; }

	[JsonProperty("completions")]
	public QuotaDetail Completions { get; set; }

	[JsonProperty("premium_interactions")]
	public QuotaDetail PremiumInteractions { get; set; }
}

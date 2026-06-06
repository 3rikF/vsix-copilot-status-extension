
using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class QuotaSnapshots
{
	[JsonProperty("chat")]
	public QuotaDetail Chat { get; set; } = new QuotaDetail();

	[JsonProperty("completions")]
	public QuotaDetail Completions { get; set; } = new QuotaDetail();

	[JsonProperty("premium_interactions")]
	public QuotaDetail PremiumInteractions { get; set; } = new QuotaDetail();
}

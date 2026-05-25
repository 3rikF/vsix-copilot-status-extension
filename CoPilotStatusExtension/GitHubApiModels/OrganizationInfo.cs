using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class OrganizationInfo
{
	[JsonProperty("login")]
	public string Login { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }
}

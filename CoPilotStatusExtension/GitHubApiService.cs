
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension;

//-----------------------------------------------------------------------------------------------------------------------------------------
internal sealed class GitHubApiService
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	//private static readonly HttpClient HttpClientInstance = CreateHttpClient();

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Token

	///// <summary>
	///// Tries to retrieve a GitHub token.
	///// Priority: GH_TOKEN → GITHUB_TOKEN → git credential manager
	///// Returns null if no token is available.
	///// </summary>
	//public Task<(string username, string token)> GetTokenAsync()
	//{
	//	// 1. Environment variables (set by gh CLI or user)
	//	//string? token = Environment.GetEnvironmentVariable("GH_TOKEN")
	//	//	?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
	//	//
	//	//if (!string.IsNullOrEmpty(token))
	//	//	return Task.FromResult<string?>(token);
	//
	//	// 2. Ask git credential manager
	//	(string? username, string? token) = TryGetTokenFromGitCredentialManager();
	//
	//	return Task.FromResult((
	//		string.IsNullOrWhiteSpace(username)	? string.Empty : username!
	//		, string.IsNullOrWhiteSpace(token)	? string.Empty : token!
	//		));
	//}

	//private static (string? username, string? token) TryGetTokenFromGitCredentialManager()
	//{
	//	try
	//	{
	//		ProcessStartInfo psi = new("git", "credential fill")
	//		{
	//			RedirectStandardInput	= true,
	//			RedirectStandardOutput	= true,
	//			RedirectStandardError	= true,
	//			UseShellExecute			= false,
	//			CreateNoWindow			= true,
	//		};
	//
	//		using Process process = Process.Start(psi)!;
	//		process.StandardInput.WriteLine("protocol=https");
	//		process.StandardInput.WriteLine("host=github.com");
	//		process.StandardInput.WriteLine(string.Empty);
	//		process.StandardInput.Close();
	//
	//		string output = process.StandardOutput.ReadToEnd();
	//		_ = process.WaitForExit(3000);
	//
	//		string[] outputLines = output.Split('\n');
	//
	//		return (
	//			ParseUSernameFromGitCredentialOutput(outputLines)
	//			, ParseTokenFromGitCredentialOutput(outputLines)
	//			);
	//	}
	//	catch
	//	{
	//		// git not available or credential manager not configured
	//	}
	//
	//	return (null, null);
	//}

	//private static string? ParseUSernameFromGitCredentialOutput(string[] outputLines)
	//{
	//	return outputLines
	//		.FirstOrDefault(line => line.StartsWith("username=", StringComparison.OrdinalIgnoreCase))
	//		?.Substring("username=".Length)
	//		.Trim();
	//}
	//
	//private static string? ParseTokenFromGitCredentialOutput(string[] outputLines)
	//{
	//	return outputLines
	//		.FirstOrDefault(line => line.StartsWith("password=", StringComparison.OrdinalIgnoreCase))
	//		?.Substring("password=".Length)
	//		.Trim();
	//}

	#endregion Token

	//-----------------------------------------------------------------------------------------------------------------
	#region API

	///// <summary>
	///// Fetches GitHub rate-limit and Copilot seat data for the given token.
	///// </summary>
	//public async Task<GitHubStatusData> FetchStatusAsync(string username, string token)
	//{
	//	GitHubStatusData result = new()
	//	{
	//		GitHubUsername = username,
	//	};
	//
	//	try
	//	{
	//		using HttpRequestMessage rateLimitReq	= BuildRequest(HttpMethod.Get, "https://api.github.com/rate_limit", token);
	//		// this API does not exist
	//		//using HttpRequestMessage copilotReq		= BuildRequest(HttpMethod.Get, "https://api.github.com/user/copilot", token);
	//
	//		Task<HttpResponseMessage> rateLimitTask	= HttpClientInstance.SendAsync(rateLimitReq);
	//		//Task<HttpResponseMessage> copilotTask	= HttpClientInstance.SendAsync(copilotReq);
	//
	//		_ = await Task.WhenAll(rateLimitTask/*, copilotTask*/);
	//
	//		using HttpResponseMessage rateLimitResp	= rateLimitTask.Result;
	//		//using HttpResponseMessage copilotResp	= copilotTask.Result;
	//
	//		//--- GitHub Rate Limits ----------------------------------------------------------
	//		if (rateLimitResp.StatusCode == HttpStatusCode.OK)
	//		{
	//			string rateLimitJson = await rateLimitResp.Content.ReadAsStringAsync();
	//			JObject rl = JObject.Parse(rateLimitJson);
	//
	//			result.CoreRemaining	= rl["resources"]?["core"]?["used"]?.Value<int>()		?? 0;
	//			result.CoreLimit		= rl["resources"]?["core"]?["limit"]?.Value<int>()		?? 0;
	//			result.SearchRemaining	= rl["resources"]?["search"]?["used"]?.Value<int>()		?? 0;
	//			result.SearchLimit		= rl["resources"]?["search"]?["limit"]?.Value<int>()	?? 0;
	//		}
	//
	//		//--- GitHub Copilot --------------------------------------------------------------
	//		// this API is not available
	//		//if (copilotResp.StatusCode == HttpStatusCode.OK)
	//		//{
	//		//	string copilotJson = await copilotResp.Content.ReadAsStringAsync();
	//		//	JObject cp = JObject.Parse(copilotJson);
	//		//
	//		//	result.CopilotActive	= true;
	//		//	result.CopilotPlanType	= cp["plan_type"]?.Value<string>() ?? "Copilot";
	//		//}
	//		//else if (copilotResp.StatusCode == HttpStatusCode.Forbidden || copilotResp.StatusCode == HttpStatusCode.NotFound)
	//		//{
	//		//	// No Copilot seat — not an error
	//		//	result.CopilotActive = false;
	//		//}
	//		//else
	//		//{
	//		//	result.ErrorMessage = $"Copilot API: {(int)copilotResp.StatusCode}";
	//		//}
	//	}
	//	catch (Exception ex)
	//	{
	//		result.ErrorMessage = ex.Message;
	//	}
	//
	//	return result;
	//}

	#endregion API

	//-----------------------------------------------------------------------------------------------------------------
	//#region Helpers
	//
	//private static HttpClient CreateHttpClient()
	//{
	//	HttpClient client = new();
	//	client.DefaultRequestHeaders.UserAgent.Add(
	//		new ProductInfoHeaderValue("vsix-status-bar", "1.0"));
	//	client.Timeout = TimeSpan.FromSeconds(10);
	//	return client;
	//}
	//
	//private static HttpRequestMessage BuildRequest(HttpMethod method, string url, string token)
	//{
	//	HttpRequestMessage req = new(method, url);
	//	req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
	//	req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
	//	req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
	//	return req;
	//}
	//
	//#endregion Helpers
}

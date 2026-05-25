
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension;

//-----------------------------------------------------------------------------------------------------------------------------------------
internal sealed class GitHubApiService
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private static readonly HttpClient HttpClientInstance = CreateHttpClient();

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

	/// <summary>
	/// Fetches Copilot billing usage for the given GitHub user.
	/// Mirrors the TypeScript fetchUserBillingUsage() function.
	/// Token requires "Plan: read-only" scope.
	/// </summary>
	public async Task<CopilotBillingUsage> FetchUserBillingUsageAsync(
		string username,
		string token,
		int? year  = null,
		int? month = null)
	{
		CopilotBillingUsage result = new ();

		try
		{
			DateTime now	= DateTime.UtcNow;
			int reqYear		= year  ?? now.Year;
			int reqMonth	= month ?? now.Month;

			string url =
				$"https://api.github.com/users/{Uri.EscapeDataString(username)}"
				+ $"/settings/billing/usage"
				+ $"?year={reqYear}&month={reqMonth}";

			using HttpRequestMessage  req	= BuildRequest(HttpMethod.Get, url, token);
			using HttpResponseMessage res	= await HttpClientInstance.SendAsync(req).ConfigureAwait(false);

			string json						= await res.Content.ReadAsStringAsync().ConfigureAwait(false);

			if (!res.IsSuccessStatusCode)
			{
				result.ErrorMessage = $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}";
				return result;
			}

			JObject root = JObject.Parse(json);

			if (root["usageItems"] is not JArray usageItems)
				return result;

			//--- filter for Copilot items only (same logic as TS source) ---
			foreach (JToken item in usageItems)
			{
				string? product = item["product"]?.Value<string>();
				if (!string.Equals(product, "copilot", StringComparison.OrdinalIgnoreCase))
					continue;

				double netAmount    = item["netAmount"]?.Value<double>()		?? 0;
				double quantity     = item["quantity"]?.Value<double>()			?? 0;
				double discount     = item["discountAmount"]?.Value<double>()	?? 0;
				double pricePerUnit = item["pricePerUnit"]?.Value<double>()		?? 0;

				result.TotalNetAmount += netAmount;
				result.TotalQuantity  += quantity;

				//--- derive included units from discount / pricePerUnit (guard div-by-zero) ---
				if (pricePerUnit > 0)
					result.TotalIncludedQuantity += Math.Round(discount / pricePerUnit);
			}

			result.TotalOverageQuantity = Math.Max(0, result.TotalQuantity - result.TotalIncludedQuantity);
		}
		catch (Exception ex)
		{
			result.ErrorMessage = ex.Message;
			Debug.WriteLine($"[GitHubApiService] FetchUserBillingUsageAsync error: {ex.Message}");
		}

		return result;
	}

	public async Task<CopilotChatStatistics> FetchUserChatUsageAsync(
		string username,
		string token)
	{
		string url						= "https://api.github.com/copilot_internal/user";
		using HttpRequestMessage  req	= BuildRequest(HttpMethod.Get, url, token);
		using HttpResponseMessage res	= await HttpClientInstance.SendAsync(req).ConfigureAwait(false);

		string json						= await res.Content.ReadAsStringAsync().ConfigureAwait(false);

		CopilotChatStatistics result = new ();

		if (!res.IsSuccessStatusCode)
		{
			result.ErrorMessage = $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}";
			return result;
		}

		CopilotQuotaResponse response = JsonConvert.DeserializeObject<CopilotQuotaResponse>(json)
			?? new CopilotQuotaResponse();

		result.QuotaSnapshots		= response?.QuotaSnapshots;
		result.CopilotPlan			= response?.CopilotPlan;
		result.QuotaResetDate		= response?.QuotaResetDate;
		result.QuotaResetDateUtc	= response?.QuotaResetDateUtc;

		return result;
	}

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
	#region Helpers

	private static HttpClient CreateHttpClient()
	{
		HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue("vsix-copilot-status-bar", "0.1"));
		client.Timeout = TimeSpan.FromSeconds(10);
		return client;
	}

	private static HttpRequestMessage BuildRequest(HttpMethod method, string url, string token)
	{
		HttpRequestMessage req = new(method, url);
		req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
		req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
		return req;
	}

	#endregion Helpers
}

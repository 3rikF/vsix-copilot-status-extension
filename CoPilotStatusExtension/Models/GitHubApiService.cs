
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using CoPilotStatusExtension.GitHubApiModels;

using Newtonsoft.Json;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
internal sealed class GitHubApiService
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private static readonly HttpClient HttpClientInstance = CreateHttpClient();

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region API

	/// <summary>
	/// Fetches Copilot billing usage for the given GitHub user.
	/// Mirrors the TypeScript fetchUserBillingUsage() function.
	/// Token requires "Plan: read-only" scope.
	/// </summary>
	public async Task<(int StatusCode, string ReasonPhrase, CopilotBillingUsage? Quota)> FetchUserBillingUsageAsync(
		string username,
		string personalAccessToken,
		int? year  = null,
		int? month = null)
	{
		CopilotBillingUsage result = new ();

		try
		{
			DateTime now	= DateTime.UtcNow;
			int reqYear		= year  ?? now.Year;
			int reqMonth	= month ?? now.Month;

			string apiEndpoint =
				$"https://api.github.com/users/{Uri.EscapeDataString(username)}"
				+ $"/settings/billing/usage"
				+ $"?year={reqYear}"
				+ $"&month={reqMonth}";

			using HttpRequestMessage  req	= BuildRequest(HttpMethod.Get, apiEndpoint, personalAccessToken);
			using HttpResponseMessage res	= await HttpClientInstance.SendAsync(req).ConfigureAwait(false);

			string serializedJsonPayload	= await res.Content.ReadAsStringAsync().ConfigureAwait(false);

			if (!res.IsSuccessStatusCode)
				return ((int)res.StatusCode, res.ReasonPhrase, result);

			else
			{
				UserBillingUsageResponse? response = JsonConvert.DeserializeObject<UserBillingUsageResponse>(serializedJsonPayload);

				if (response?.UsageItems is not { } usageItems)
					return ((int)res.StatusCode, "Invalid response format: missing 'usageItems' array", result);

				//--- filter for Copilot items only (same logic as TS source) ---
				foreach (UserBillingUsageItem item in usageItems)
				{
					if (!string.Equals(item.Product, "copilot", StringComparison.OrdinalIgnoreCase))
						continue;

					result.TotalNetAmount	+= item.NetAmount;
					result.TotalQuantity	+= item.Quantity;

					//--- derive included units from discount / pricePerUnit (guard div-by-zero) ---
					if (item.PricePerUnit > 0)
						result.TotalIncludedQuantity += Math.Round(item.DiscountAmount / item.PricePerUnit);
				}

				result.TotalOverageQuantity = Math.Max(0, result.TotalQuantity - result.TotalIncludedQuantity);
				return ((int)res.StatusCode, res.ReasonPhrase, result);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error fetching billing usage: {ex}");
			return (-1, $"Exception: {ex.Message}", result);
		}
	}

	public async Task<(int StatusCode, string ReasonPhrase, CopilotQuotaResponse? Quota, RateLimitInfo? RateLimit)> FetchUserChatUsageAsync(string personalAccessToken)
	{
		const string API_ENDPOINT		= "https://api.github.com/copilot_internal/user";
		using HttpRequestMessage  req	= BuildRequest(HttpMethod.Get, API_ENDPOINT, personalAccessToken);
		using HttpResponseMessage res	= await HttpClientInstance.SendAsync(req).ConfigureAwait(false);

		string serializedJsonPayload	= await res.Content.ReadAsStringAsync().ConfigureAwait(false);
		RateLimitInfo? rateLimit		= RateLimitInfo.FromHeaders(res.Headers);

		if (!res.IsSuccessStatusCode)
			return ((int)res.StatusCode, res.ReasonPhrase, null, rateLimit);
		else
		{
			return
				(
					(int)res.StatusCode
					, res.ReasonPhrase
					, JsonConvert.DeserializeObject<CopilotQuotaResponse>(serializedJsonPayload)
					, rateLimit
				);
		}
	}

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
		//req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
		req.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
		return req;
	}

	#endregion Helpers
}

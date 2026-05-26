
// ignore spelling: ratelimit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.GitHubApiModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
/// <summary>
/// Rate-limit information extracted from GitHub API response headers.
/// </summary>
public sealed class RateLimitInfo
{
	/// <summary>The maximum number of requests that you can make per hour.</summary>
	public int Limit { get; set; }

	/// <summary>The number of requests remaining in the current rate limit window.</summary>
	public int Remaining { get; set; }

	/// <summary>The number of requests made in the current rate limit window.</summary>
	public int Used { get; set; }

	/// <summary>The time at which the current rate limit window resets (UTC).</summary>
	public DateTime? Reset { get; set; }

	/// <summary>The rate limit resource that the request counted against.</summary>
	public string? Resource { get; set; }

	/// <summary>
	/// Extracts rate-limit values from the HTTP response headers.
	/// Returns <c>null</c> when none of the expected headers are present.
	/// </summary>
	/// <seealso cref="https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api?apiVersion=2026-03-10"/>
	public static RateLimitInfo? FromHeaders(HttpResponseHeaders headers)
	{
		if (headers is null)
			return null;

		bool any = false;
		RateLimitInfo info = new();

		if (TryGetInt(headers, "x-ratelimit-limit", out int limit))
		{
			info.Limit		= limit;
			any				= true;
		}

		if (TryGetInt(headers, "x-ratelimit-remaining", out int remaining))
		{
			info.Remaining	= remaining;
			any				= true;
		}

		if (TryGetInt(headers, "x-ratelimit-used", out int used))
		{
			info.Used		= used;
			any				= true;
		}

		if (TryGetLong(headers, "x-ratelimit-reset", out long epochSeconds))
		{
			info.Reset		= DateTimeOffset.FromUnixTimeSeconds(epochSeconds).ToLocalTime().DateTime;
			any				= true;
		}

		if (TryGetString(headers, "x-ratelimit-resource", out string? resource))
		{
			info.Resource	= resource;
			any				= true;
		}

		return any
			? info
			: null;
	}

	//----------------------------------------------------------------------------------------------------------
	#region Helpers

	private static bool TryGetInt(HttpResponseHeaders headers, string name, out int out_value)
	{
		out_value = 0;

		return headers.TryGetValues(name, out IEnumerable<string> values)
			&& values.Any()
			&& int.TryParse(values.First(), out out_value);
	}

	private static bool TryGetLong(HttpResponseHeaders headers, string name, out long out_value)
	{
		out_value = 0;

		return headers.TryGetValues(name, out IEnumerable<string> values)
			&& values.Any()
			&& long.TryParse(values.First(), out out_value);
	}

	private static bool TryGetString(HttpResponseHeaders headers, string name, out string? out_value)
	{
		out_value = null;

		return headers.TryGetValues(name, out IEnumerable<string> values)
			&& values.Any()
			&& (out_value = values.First()) is not null;
	}

	#endregion Helpers
}

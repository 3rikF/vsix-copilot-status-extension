
// ignore spelling: bindable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Helper;
using CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class GitHubStatusBarViewModel : INotifyPropertyChanged
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private CopilotBillingUsage?	_githubBillingUsage;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region UI Bindable Properties

	public bool HasValidUser
		=> !string.IsNullOrWhiteSpace(PersonalInfo?.Login ?? AccountInfo?.Username);

	public bool HasValidToken
		=> !string.IsNullOrWhiteSpace(PersonalInfo?.Login);

	public bool IsPrivateUser
		=> PersonalInfo?.CopilotPlan?.Equals("individual", StringComparison.OrdinalIgnoreCase) ?? false;

	/// <summary>
	/// The text shown in the status  bar item.
	/// Contains either the username + percentage or a status like "Not Signed In".
	/// </summary>
	public string StatusText
		{ get; private set; } = string.Empty;

	public string ExtensionNameAndVersion
		{ get;} = $"{CoPilotStatusExtensionPackage.EXTENSION_NAME} v{VersionHelper.GetExtensionVersion("?.?.?")}";

	/// <summary>
	/// Premium interactions used as a fraction in the range [0, 1] (null if unavailable).
	/// </summary>
	public double? PremiumInteractionsUsedPercent
		=> PersonalInfo?.QuotaSnapshots?.PremiumInteractions?.PercentUsed;

	/// <summary>
	/// Fraction of the current monthly premium-interactions period that has elapsed.
	/// The quota API exposes the next reset instant, so the start of the period is
	/// derived as the previous monthly reset.
	/// </summary>
	public double? CurrentPeriodElapsedPercent
		=> CalculateCurrentPeriodElapsedPercent(PremiumInteractionsResetDateUtc, DateTimeOffset.UtcNow);

	public string UsagePaceStatusText
	{
		get
		{
			if (PremiumInteractionsUsedPercent is not double usage
				|| CurrentPeriodElapsedPercent is not double elapsed)
			{
				return string.Empty;
			}

			double difference = usage - elapsed;
			if (Math.Abs(difference) < 0.005d)
				return "On pace";

			return difference > 0
				? $"Ahead of pace by {difference:P1}"
				: $"Behind pace by {-difference:P1}";
		}
	}

	public IEnumerable<QuotaDetail> QuotaDetails
	{
		get
		{
			if (PersonalInfo?.QuotaSnapshots is null)
				yield break;

			if (PersonalInfo.QuotaSnapshots.PremiumInteractions is not null)
				yield return PersonalInfo.QuotaSnapshots.PremiumInteractions;

			if (PersonalInfo.QuotaSnapshots.Chat is not null)
				yield return PersonalInfo.QuotaSnapshots.Chat;

			if (PersonalInfo.QuotaSnapshots.Completions is not null)
				yield return PersonalInfo.QuotaSnapshots.Completions;
		}
	}

	public CopilotUserInfo? AccountInfo
		{get; private set; } = null;

	public CopilotQuotaResponse? PersonalInfo
		{get; private set; } = null;

	public RateLimitInfo? ApiRateLimitInfo
		{get; private set; } = null;

	public string? UserProfileUrl
	{
		get
		{
			string? login = PersonalInfo?.Login;
			return string.IsNullOrEmpty(login)
				? null
				: $"https://github.com/{login}";
		}
	}

	#endregion UI Bindable Properties

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit)
	{
		AccountInfo				= copilotUserInfo;
		_githubBillingUsage		= billingUsage;
		PersonalInfo			= personalQuota;
		ApiRateLimitInfo		= apiRateLimit;

		StatusText				= GetStatusText();

		RaiseAllPropertyChanged();
	}

	private string GetStatusText()
	{
		StringBuilder sb = new();

		//-------------------------------------------------
		string? userName = PersonalInfo?.Login
			?? AccountInfo?.Username;

		if (!string.IsNullOrEmpty(userName))
			_ = sb.Append(userName);

		//-------------------------------------------------
		else if (AccountInfo?.Status?.Equals("NotSignedInToGitHub", StringComparison.OrdinalIgnoreCase) == true)
			_ = sb.Append("Not signed in");
		else
			_ = sb.Append("Status not available");

		//-------------------------------------------------
		if (PremiumInteractionsUsedPercent is not null)
			_ = sb.Append($": {PremiumInteractionsUsedPercent:P1}");
		//-------------------------------------------------
		return sb.ToString();
	}

	private DateTimeOffset? PremiumInteractionsResetDateUtc
		=> GetPremiumInteractionsResetDateUtc(PersonalInfo);

	internal static DateTimeOffset? GetPremiumInteractionsResetDateUtc(CopilotQuotaResponse? personalQuota)
	{
		long quotaResetAt = personalQuota?.QuotaSnapshots?.PremiumInteractions?.QuotaResetAt ?? 0;
		if (quotaResetAt > 0)
		{
			try
			{
				return DateTimeOffset.FromUnixTimeSeconds(quotaResetAt);
			}
			catch (ArgumentOutOfRangeException)
			{
				// Fall back to the response-level reset date below.
			}
		}

		DateTimeOffset responseResetDate = personalQuota?.QuotaResetDateUtc ?? default;
		return responseResetDate == default ? null : responseResetDate;
	}

	/// <summary>
	/// Calculates the elapsed fraction of the monthly period ending at <paramref name="periodEnd"/>.
	/// Kept independent of the system clock so its boundary behavior can be verified deterministically.
	/// </summary>
	internal static double? CalculateCurrentPeriodElapsedPercent(DateTimeOffset? periodEnd, DateTimeOffset now)
	{
		if (periodEnd is null)
			return null;

		DateTimeOffset periodStart = periodEnd.Value.AddMonths(-1);
		if (now <= periodStart)
			return 0d;

		if (now >= periodEnd.Value)
			return 1d;

		return (now - periodStart).TotalSeconds
			/ (periodEnd.Value - periodStart).TotalSeconds;
	}

	#endregion Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region INotifyPropertyChanged

	protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
	{
		if (Equals(field, newValue))
		{
			return false;
		}

		else
		{
			field = newValue;
			RaisePropertyChanged(propertyName);

			return true;
		}
	}

	protected bool SetProperty<T>(ref T field, T newValue, params string[] additionalDependentProperties)
	{
		if (Equals(field, newValue))
		{
			return false;
		}

		else
		{
			field = newValue;

			foreach (string dependentProperty in additionalDependentProperties)
				RaisePropertyChanged(dependentProperty);

			return true;
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected virtual void RaiseAllPropertyChanged()
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

	#endregion
}

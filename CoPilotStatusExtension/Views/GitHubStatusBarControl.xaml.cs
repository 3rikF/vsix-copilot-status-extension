
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views;

//-----------------------------------------------------------------------------------------------------------------------------------------
public enum EStatusEnum
{
	Unset,
	OK,
	NotSignedInToGitHub,
	Unhandled,
}

public enum ESubscriptionType
{
	Unknown,
	free_limited_quota,
	trial_subscriber_quota,
	yearly_subscriber_quota,
	copilot_enterprise_seat_quota,
	copilot_for_business_seat_quota,
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public partial class GitHubStatusBarControl : UserControl
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	/// <summary>
	/// Contains basic user and subscription info retrieved from Copilot MEF components
	/// (like username, subscription type, chat/completions enabled flags etc).
	/// </summary>
	private CopilotUserInfo? _basicCopilotUserInfo;

	/// <summary>
	/// Contains a *very* thorough breakdown of Copilot usage for the current billing period,
	/// retrieved from GitHub's billing API.
	/// </summary>
	private CopilotBillingUsage? _githubBillingUsage;

	/// <summary>
	/// Contains personal quota and usage details retrieved from GitHub's Copilot API.
	/// That also includes the remaining quota percentage which is used to display the status text in the status bar.
	/// </summary>
	private CopilotQuotaResponse? _personalQuota;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Construction

	public GitHubStatusBarControl()
		=> InitializeComponent();

	#endregion Construction

	//-----------------------------------------------------------------------------------------------------------------
	#region Properties

	/// <summary>
	/// Backing field for the text shown in the status bar.
	/// It's derived from the remaining quota percentage, but also includes some basic status info
	/// (like username or "not signed in" message).
	/// </summary>
	public string StatusText
	{
		get => ctrlStatusTextBlock.Text;
		private set => ctrlStatusTextBlock.Text = value;
	}

	/// <summary>
	/// Backing field for the tool-tip shown when hovering over the status bar text.
	/// It contains detailed information about the user's Copilot usage and quota.
	/// </summary>
	public string StatusToolTip
	{
		get => ctrlStatusToolTip.Text?.ToString() ?? string.Empty;
		private set => ctrlStatusToolTip.Text = value;
	}

	#endregion Properties

	//-----------------------------------------------------------------------------------------------------------------
	#region Static Methods

	private static EStatusEnum ParseStatus(string? status)
	{
		if (status is null || string.IsNullOrWhiteSpace(status))
			return EStatusEnum.Unset;

		else if (status.Equals( "NotSignedInToGitHub", StringComparison.OrdinalIgnoreCase))
			return EStatusEnum.NotSignedInToGitHub;

		else if (status.Equals( "OK", StringComparison.OrdinalIgnoreCase))
			return EStatusEnum.OK;

		else
			return EStatusEnum.Unhandled;
	}

	private static ESubscriptionType ParseSubscriptionType(string? subscriptionType)
	{
		return !string.IsNullOrWhiteSpace(subscriptionType) && Enum.TryParse<ESubscriptionType>(subscriptionType, ignoreCase: true, out ESubscriptionType result)
			? result
			: ESubscriptionType.Unknown;
	}

	private static StringBuilder GetQuotaDetailToolTip(string title, QuotaDetail detail)
	{
		if (detail.Unlimited)
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"      Unlimited");
		}
		else
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"      Remaining:	{100-detail.PercentRemaining:F1}%  ({detail.QuotaRemaining} / {detail.Entitlement} left)")
				.AppendLine($"      Overage:	{detail.OverageCount} (permitted: {FormatBool(detail.OveragePermitted)})");
		}
	}

	private static string FormatBool(bool? value) => value switch
	{
		true  => "✔",
		false => "❌",
		null  => "❓",
	};

	#endregion Static Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota)
	{
		_basicCopilotUserInfo	= copilotUserInfo;
		_githubBillingUsage		= billingUsage;
		_personalQuota			= personalQuota;

		UpdateStatusText();
		UpdateToolTip();
	}

	private void UpdateStatusText()
	{
		StringBuilder sb = new ();

		string userName = _personalQuota?.Login
			?? _basicCopilotUserInfo?.Username
			?? "n/a";

		_ = sb.Append( ParseStatus(_basicCopilotUserInfo?.Status) switch
		{
			EStatusEnum.OK			=> userName,
			EStatusEnum.Unset		=> "Status not available",
			EStatusEnum.NotSignedInToGitHub => "Not signed in",
			EStatusEnum.Unhandled	=> $"Unhandled status [{_basicCopilotUserInfo?.Status}]",
			_						=> "Unknown status"
		});

		if (_personalQuota?.QuotaSnapshots?.PremiumInteractions?.PercentRemaining is double PercentRemaining)
			_ = sb.Append($": {100-PercentRemaining:F1}%");

		StatusText = sb.ToString();
	}



	private void UpdateToolTip()
	{
		StringBuilder sb = new();

		//--- helper ----------------------------------------------------------
		IEnumerable<string> EnumerateAccountTypes()
		{
			if (_basicCopilotUserInfo?.IsEnterprise == true)
				yield return "Enterprise";
			if (_basicCopilotUserInfo?.IsIndividual == true)
				yield return "Individual";
		}

		string subscriptionType = ParseSubscriptionType(_basicCopilotUserInfo?.SubscriptionType) switch
		{
			ESubscriptionType.free_limited_quota			=> "Free (limited quota)",
			ESubscriptionType.trial_subscriber_quota		=> "Trial Subscriber",
			ESubscriptionType.yearly_subscriber_quota		=> "Yearly Subscriber",
			ESubscriptionType.copilot_enterprise_seat_quota	=> "Copilot Enterprise",
			ESubscriptionType.copilot_for_business_seat_quota => "Copilot for Business",
			ESubscriptionType.Unknown						=> "Unknown",
			_ => $"[{_basicCopilotUserInfo?.SubscriptionType}]"
		};

		//--- basic info ------------------------------------------------------
		_ = _basicCopilotUserInfo is null
			? sb.AppendLine("Basic user information is not available.")
			: sb
				.AppendLine("--- Basic Information ---")
				.AppendLine($"   Status:			{_basicCopilotUserInfo.Status}")
				.AppendLine($"   User:			{_basicCopilotUserInfo.Username}")
				.AppendLine($"   Subscription:		{subscriptionType}")
				.AppendLine($"   Account type:		{string.Join(", ", EnumerateAccountTypes())}");

		//--- Quota details ---------------------------------------------------
		if (_personalQuota is not null)
		{
			_ = sb
				.AppendLine()
				.AppendLine("--- Features Enabled ---")
				.AppendLine($"   Chat:			{FormatBool(_personalQuota.ChatEnabled)}")
				.AppendLine($"   Editor preview:		{FormatBool(_personalQuota.EditorPreviewFeaturesEnabled)}")
				.AppendLine($"   CLI:			{FormatBool(_personalQuota.CliEnabled)}")
				.AppendLine($"   CLI Remote Control:	{FormatBool(_personalQuota.CliRemoteControlEnabled)}")
				.AppendLine($"   Cloud Session Storage:	{FormatBool(_personalQuota.CloudSessionStorageEnabled)}")
				.AppendLine($"   Copilot Ignore:		{FormatBool(_personalQuota.CopilotIgnoreEnabled)}")
				.AppendLine($"   MCP:			{FormatBool(_personalQuota.IsMcpEnabled)}");

			_ = sb
				.AppendLine()
				.AppendLine("--- Personal Metrics ---");

			//--- premium quota ---------------------------
			_ = _personalQuota.QuotaSnapshots?.PremiumInteractions is not null
				? sb.Append(GetQuotaDetailToolTip("   Premium Interactions", _personalQuota.QuotaSnapshots.PremiumInteractions))
				: sb.AppendLine($"   Premium Interactions: [No data]");

			//--- Chat Interactions -----------------------
			_ = _personalQuota.QuotaSnapshots?.Chat is not null
				? sb.Append(GetQuotaDetailToolTip("   Chat Interactions", _personalQuota.QuotaSnapshots.Chat))
				: sb.AppendLine($"   Chat Interactions: [No data]");

			//--- Completions -----------------------------
			_ = _personalQuota.QuotaSnapshots?.Completions is not null
				? sb.Append(GetQuotaDetailToolTip("   Completions", _personalQuota.QuotaSnapshots.Completions))
				: sb.AppendLine($"   Completions: [No data]");
		}
		else
		{
			_ = sb.AppendLine("Personal quota and usage information is not available.");
		}

		//--- return result ---------------------------------------------------
		StatusToolTip = sb.ToString();
	}

	#endregion Methods
}

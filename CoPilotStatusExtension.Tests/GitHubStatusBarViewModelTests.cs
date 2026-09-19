using System;
using System.Globalization;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.ViewModels;
using CoPilotStatusExtension.Views.Converters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Tests;

//-----------------------------------------------------------------------------------------------------------------------------------------
[TestClass]
public sealed class GitHubStatusBarViewModelTests
{
	[TestMethod]
	public void GetPremiumInteractionsResetDateUtc_PrefersPremiumInteractionsTimestamp()
	{
		DateTimeOffset premiumResetDate = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset responseResetDate = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
		CopilotQuotaResponse quota = CreateQuotaResponse(premiumResetDate, responseResetDate);

		Assert.AreEqual(premiumResetDate, GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(quota));
	}

	[TestMethod]
	public void GetPremiumInteractionsResetDateUtc_FallsBackToResponseResetDate()
	{
		DateTimeOffset responseResetDate = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
		CopilotQuotaResponse quota = CreateQuotaResponse(0, responseResetDate);

		Assert.AreEqual(responseResetDate, GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(quota));
	}

	[TestMethod]
	public void GetPremiumInteractionsResetDateUtc_FallsBackWhenPremiumTimestampIsInvalid()
	{
		DateTimeOffset responseResetDate = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
		CopilotQuotaResponse quota = CreateQuotaResponse(long.MaxValue, responseResetDate);

		Assert.AreEqual(responseResetDate, GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(quota));
	}

	[TestMethod]
	public void GetPremiumInteractionsResetDateUtc_ReturnsNullWhenNoResetDateIsAvailable()
	{
		Assert.IsNull(GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(null));
		Assert.IsNull(GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(new CopilotQuotaResponse()));
	}

	[TestMethod]
	public void CalculateCurrentPeriodElapsedPercent_ReturnsNull_WhenResetIsUnavailable()
	{
		double? elapsed = GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(null, DateTimeOffset.UtcNow);

		Assert.IsNull(elapsed);
	}

	[TestMethod]
	public void CalculateCurrentPeriodElapsedPercent_ClampsAtPeriodBoundaries()
	{
		DateTimeOffset periodEnd = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

		Assert.AreEqual(0d, GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, periodEnd.AddMonths(-1).AddTicks(-1)));
		Assert.AreEqual(0d, GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, periodEnd.AddMonths(-1)));
		Assert.AreEqual(1d, GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, periodEnd));
		Assert.AreEqual(1d, GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, periodEnd.AddTicks(1)));
	}

	[TestMethod]
	public void CalculateCurrentPeriodElapsedPercent_UsesCalendarMonthLength()
	{
		DateTimeOffset periodEnd = new(2026, 3, 31, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

		double? elapsed = GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, now);

		Assert.AreEqual(0.5d, elapsed!.Value, 0.0000001d);
	}

	[TestMethod]
	public void PercentageToCanvasLeftConverter_CentersAndClampsMarker()
	{
		PercentageToCanvasLeftConverter converter = new();

		Assert.AreEqual(0d, converter.Convert([100d, 0d], typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.AreEqual(48d, converter.Convert([100d, 0.5d], typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.AreEqual(96d, converter.Convert([100d, 1d], typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.AreEqual(0d, converter.Convert([100d, double.NaN], typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.AreEqual(0d, converter.Convert([100d, double.PositiveInfinity], typeof(double), "4", CultureInfo.InvariantCulture));
	}

	private static CopilotQuotaResponse CreateQuotaResponse(long premiumResetAt, DateTimeOffset responseResetDate)
		=> new()
		{
			QuotaResetDateUtc = responseResetDate,
			QuotaSnapshots = new QuotaSnapshots
			{
				PremiumInteractions = new QuotaDetail { QuotaResetAt = premiumResetAt },
			},
		};

	private static CopilotQuotaResponse CreateQuotaResponse(DateTimeOffset premiumResetDate, DateTimeOffset responseResetDate)
		=> CreateQuotaResponse(premiumResetDate.ToUnixTimeSeconds(), responseResetDate);
}

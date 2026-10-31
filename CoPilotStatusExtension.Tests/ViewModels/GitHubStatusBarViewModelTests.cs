using System;
using System.Globalization;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.ViewModels;
using CoPilotStatusExtension.Views.Converters;

using Xunit;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Tests.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class GitHubStatusBarViewModelTests
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Test Helper

	private static CopilotQuotaResponse CreateQuotaResponse(long premiumResetAt, DateTimeOffset responseResetDate)
	{
		return new()
		{
			QuotaResetDateUtc	= responseResetDate,
			QuotaSnapshots		= new QuotaSnapshots
			{
				PremiumInteractions = new ()
				{
					QuotaResetAt	= premiumResetAt
				},
			},
		};
	}

	#endregion Test Helper

	//-----------------------------------------------------------------------------------------------------------------
	#region Test Data

	public static TheoryData<DateTimeOffset, DateTimeOffset, double> CalculateCurrentPeriodElapsedPercentTestData()
	{
		DateTimeOffset periodEnd = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
		return new()
		{
			{ periodEnd, periodEnd.AddMonths(-1).AddTicks(-1),	0d },
			{ periodEnd, periodEnd.AddMonths(-1),				0d },
			{ periodEnd, periodEnd,								1d },
			{ periodEnd, periodEnd.AddTicks(1),					1d },
		};
	}

	#endregion Test Data

	//-----------------------------------------------------------------------------------------------------------------
	#region Meta Tests

	[Fact]
	public void CalculateCurrentPeriodElapsedPercentTestData_ContainsTestData()
		=> Assert.NotEmpty(CalculateCurrentPeriodElapsedPercentTestData());

	#endregion Meta Tests

	//-----------------------------------------------------------------------------------------------------------------
	#region Test Methods: GetPremiumInteractionsResetDateUtc

	[Fact]
	public void GetPremiumInteractionsResetDateUtc_PrefersPremiumInteractionsTimestamp()
	{
		//---- ARRANGE --------------------------------------------------------
		DateTimeOffset premiumResetDate		= new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset responseResetDate	= new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);

		//---- ACT ------------------------------------------------------------
		CopilotQuotaResponse quota = CreateQuotaResponse(premiumResetDate.ToUnixTimeSeconds(), responseResetDate);

		//---- ASSERT ---------------------------------------------------------
		Assert.Equal(premiumResetDate, GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(quota));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(long.MaxValue)]
	public void GetPremiumInteractionsResetDateUtc_FallsBackWhenPremiumTimestampIsInvalid(long testUnixSeconds)
	{
		//---- ARRANGE --------------------------------------------------------
		DateTimeOffset responseResetDate = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);

		//---- ACT ------------------------------------------------------------
		CopilotQuotaResponse quota = CreateQuotaResponse(testUnixSeconds, responseResetDate);

		//---- ASSERT ---------------------------------------------------------
		Assert.Equal(responseResetDate, GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(quota));
	}

	[Fact]
	public void GetPremiumInteractionsResetDateUtc_ReturnsNullWhenNoResetDateIsAvailable()
	{
		//---- ARRANGE --------------------------------------------------------
		//---- ACT ------------------------------------------------------------
		//---- ASSERT ---------------------------------------------------------
		Assert.Null(GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(null));
		Assert.Null(GitHubStatusBarViewModel.GetPremiumInteractionsResetDateUtc(new CopilotQuotaResponse()));
	}

	[Fact]
	public void CalculateCurrentPeriodElapsedPercent_ReturnsNull_WhenResetIsUnavailable()
	{
		//---- ARRANGE --------------------------------------------------------
		//---- ACT ------------------------------------------------------------
		double? elapsed = GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(null, DateTimeOffset.UtcNow);

		//---- ASSERT ---------------------------------------------------------
		Assert.Null(elapsed);
	}

	[Theory]
	[MemberData(nameof(CalculateCurrentPeriodElapsedPercentTestData))]
	public void CalculateCurrentPeriodElapsedPercent_ClampsAtPeriodBoundaries(DateTimeOffset testPeriodEnd, DateTimeOffset testNowDateTime, double expectedElapsedPercent)
	{
		//---- ARRANGE --------------------------------------------------------
		//---- ACT ------------------------------------------------------------
		//---- ASSERT ---------------------------------------------------------
		Assert.Equal(expectedElapsedPercent, GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(testPeriodEnd, testNowDateTime));
	}

	[Fact]
	public void CalculateCurrentPeriodElapsedPercent_UsesCalendarMonthLength()
	{
		//---- ARRANGE --------------------------------------------------------
		DateTimeOffset periodEnd	= new(2026, 3, 31, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset now			= new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

		//---- ACT ------------------------------------------------------------
		double? elapsed = GitHubStatusBarViewModel.CalculateCurrentPeriodElapsedPercent(periodEnd, now);

		//---- ASSERT ---------------------------------------------------------
		Assert.Equal(0.5d, elapsed!.Value, 0.0000001d);
	}

	#endregion Test Methods
}

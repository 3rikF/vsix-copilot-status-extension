
using System;
using System.Globalization;

using CoPilotStatusExtension.Views.Converters;

using Xunit;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Tests.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class PercentageToCanvasLeftConverterTests
{
	[Theory]
	//--- cover valid and edge cases for the percentage value, which is expected to be in the range of 0.0 to 1.0 ---
	[InlineData(0d,							"4",		0d)]	// == 100 * 0.0			- 0
	[InlineData(0.5d,						"4",		48d)]	// == 100 * 0.5			- 4/2
	[InlineData(1d,							"4",		96d)]	// == 100 * 1.0			- 4
	[InlineData(double.NaN,					"4",		0d)]	// == 100 * NaN			- 4/
	[InlineData(double.PositiveInfinity,	"4",		0d)]    // == 100 * Infinity	- 4/
	//--- cover invalid marker-width values, which will be ignored and replaced by the default value of 3
	[InlineData(0.5d,						"invalid",	48.5d)]	// == 100 * 0.5		- (Default-Width)/2
	[InlineData(0.5d,						"-4",		48.5d)]	// == 100 * 0.5		- (Default-Width)/2
	[InlineData(0.5d,						"0",		48.5d)]	// == 100 * 0.5		- (Default-Width)/2
	public void Convert_ReturnsExpectedValues(double testPercentage, string testMarkerWidthText, double expectedMarkerPos)
	{
		//---- ARRANGE --------------------------------------------------------
		PercentageToCanvasLeftConverter converter = new();

		//---- ACT & ASSERT ---------------------------------------------------
		//                                                Width  Percentage			              Marker-Width
		Assert.Equal(expectedMarkerPos, converter.Convert([100d, testPercentage], typeof(double), testMarkerWidthText, CultureInfo.InvariantCulture));
	}

	[Fact]
	public void ConvertBack_ThrowsNotException()
	{
		//---- ARRANGE --------------------------------------------------------
		PercentageToCanvasLeftConverter converter = new();

		//---- ACT & ASSERT -----------------------------------------------------
		_ = Assert.Throws<NotSupportedException>(
			() => converter.ConvertBack(0d, [typeof(double)], "4", CultureInfo.InvariantCulture));
	}
}


using System.Globalization;

using CoPilotStatusExtension.Views.Converters;

using Xunit;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Tests.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class PercentageToCanvasLeftConverterTests
{
	[Fact]
	public void PercentageToCanvasLeftConverter_CentersAndClampsMarker()
	{
		//---- ARRANGE --------------------------------------------------------
		PercentageToCanvasLeftConverter converter = new();

		//---- ACT & ASSERT ---------------------------------------------------
		Assert.Equal(0d,	converter.Convert([100d, 0d],						typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.Equal(48d,	converter.Convert([100d, 0.5d],						typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.Equal(96d,	converter.Convert([100d, 1d],						typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.Equal(0d,	converter.Convert([100d, double.NaN],				typeof(double), "4", CultureInfo.InvariantCulture));
		Assert.Equal(0d,	converter.Convert([100d, double.PositiveInfinity],	typeof(double), "4", CultureInfo.InvariantCulture));
	}
}

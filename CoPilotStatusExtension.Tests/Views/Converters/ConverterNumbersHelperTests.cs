
// ignore spelling: abc

using CoPilotStatusExtension.Views.Converters;

using Xunit;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Tests.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class ConverterNumbersHelperTests(ITestOutputHelper TestConsole)
{
	[Theory]
	[InlineData(1,						true)]
	[InlineData(1.0,					false)]
	[InlineData(1.0f,					false)]
	[InlineData(1L,						true)]
	[InlineData((short)1,				true)]
	[InlineData((byte)1,				true)]
	[InlineData(null,					false)]
	[InlineData("1",					false)]
	[InlineData("1.0",					false)]
	[InlineData("1e3",					false)]
	[InlineData("1e-3",					false)]
	[InlineData("abc",					false)]
	[InlineData(new[] { "abc", "def" },	false)]
	public void IsInteger_ReturnsExpected(object? value, bool expected)
	{
		//--- ARRANGE ---------------------------------------------------------
		TestConsole.WriteLine($"Test value:      {value}");

		//--- ACT -------------------------------------------------------------
		bool result = ConverterNumbersHelper.IsInteger(value!);

		TestConsole.WriteLine($"Expected Result: {result}");
		TestConsole.WriteLine($"Actual Result:   {expected}");

		//--- ASSERT ----------------------------------------------------------
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1,						false)]
	[InlineData(1.0,					true)]
	[InlineData(1.0f,					true)]
	[InlineData(1L,						false)]
	[InlineData((short)1,				false)]
	[InlineData((byte)1,				false)]
	[InlineData(null,					false)]
	[InlineData("1",					false)]
	[InlineData("1.0",					false)]
	[InlineData("1e3",					false)]
	[InlineData("1e-3",					false)]
	[InlineData("abc",					false)]
	[InlineData(new[] { "abc", "def" },	false)]
	public void IsReal_ReturnsExpected(object? value, bool expected)
	{
		//--- ARRANGE ---------------------------------------------------------
		TestConsole.WriteLine($"Test value:      {value}");

		//--- ACT -------------------------------------------------------------
		bool result = ConverterNumbersHelper.IsReal(value!);

		TestConsole.WriteLine($"Expected Result: {result}");
		TestConsole.WriteLine($"Actual Result:   {expected}");

		//--- ASSERT ----------------------------------------------------------
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1,						true)]
	[InlineData(1.0,					true)]
	[InlineData(1.0f,					true)]
	[InlineData(1L,						true)]
	[InlineData((short)1,				true)]
	[InlineData((byte)1,				true)]
	[InlineData(null,					false)]
	[InlineData("1",					false)]
	[InlineData("1.0",					false)]
	[InlineData("1e3",					false)]
	[InlineData("1e-3",					false)]
	[InlineData("abc",					false)]
	[InlineData(new[] { "abc", "def" },	false)]
	public void IsNumber_ReturnsExpected(object? value, bool expected)
	{
		//--- ARRANGE ---------------------------------------------------------
		TestConsole.WriteLine($"Test value:      {value}");

		//--- ACT -------------------------------------------------------------
		bool result = ConverterNumbersHelper.IsNumber(value!);

		TestConsole.WriteLine($"Expected Result: {result}");
		TestConsole.WriteLine($"Actual Result:   {expected}");

		//--- ASSERT ----------------------------------------------------------
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1,			true, 1)]
	[InlineData(1.0,		true, 1.0)]
	[InlineData(1.0f,		true, 1.0f)]
	[InlineData(1L,			true, 1L)]
	[InlineData((short)1,	true, (short)1)]
	[InlineData((byte)1,	true, (byte)1)]
	[InlineData("1",		true, 1.0)]
	[InlineData("1.5",		true, 1.5)]
	[InlineData("abc",		false, 0D)]
	[InlineData(null,		false, 0D)]
	public void TryGetNumber_ReturnsExpected(object? value, bool expectedSuccess, object expectedNumber)
	{
		//--- ARRANGE ---------------------------------------------------------
		TestConsole.WriteLine($"Test value:       {value}");

		//--- ACT -------------------------------------------------------------
		bool success = ConverterNumbersHelper.TryGetNumber(value, out dynamic? number);

		TestConsole.WriteLine($"Expected Success: {expectedSuccess}");
		TestConsole.WriteLine($"Actual Success:   {success}");

		//--- ASSERT ----------------------------------------------------------
		Assert.Equal(expectedSuccess,	success);
		Assert.Equal(expectedNumber,	number);
	}
}

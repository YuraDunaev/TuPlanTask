using CadBridge.Core.Models;
using CadBridge.Core.Serialization;

namespace CadBridge.Core.Tests.Serialization;

public sealed class WarningCodeMapperTests
{
    [Theory]
    [InlineData(WarningCode.OpenPolyline, "OPEN_POLYLINE")]
    [InlineData(WarningCode.NoTag, "NO_TAG")]
    [InlineData(WarningCode.MultipleTags, "MULTIPLE_TAGS")]
    [InlineData(WarningCode.OrphanTag, "ORPHAN_TAG")]
    [InlineData(WarningCode.CalculationFailed, "CALCULATION_FAILED")]
    public void ToWire_MapsEveryKnownCode(WarningCode code, string expected)
    {
        Assert.Equal(expected, WarningCodeMapper.ToWire(code));
    }

    [Fact]
    public void ToWire_UnknownCode_BecomesUnknown()
    {
        Assert.Equal("UNKNOWN", WarningCodeMapper.ToWire(WarningCode.UnknownCode));
    }

    [Theory]
    [InlineData("OPEN_POLYLINE", WarningCode.OpenPolyline)]
    [InlineData("NO_TAG", WarningCode.NoTag)]
    [InlineData("MULTIPLE_TAGS", WarningCode.MultipleTags)]
    [InlineData("ORPHAN_TAG", WarningCode.OrphanTag)]
    [InlineData("CALCULATION_FAILED", WarningCode.CalculationFailed)]
    public void FromWire_ParsesScreamingSnake(string wire, WarningCode expected)
    {
        Assert.Equal(expected, WarningCodeMapper.FromWire(wire));
    }

    [Fact]
    public void FromWire_IsCaseAndWhitespaceInsensitive()
    {
        Assert.Equal(WarningCode.NoTag, WarningCodeMapper.FromWire("  no_tag "));
        Assert.Equal(WarningCode.OpenPolyline, WarningCodeMapper.FromWire("OPEN_POLYLINE"));
    }

    [Fact]
    public void FromWire_NullAndUnknown_BecomeUnknownCode()
    {
        Assert.Equal(WarningCode.UnknownCode, WarningCodeMapper.FromWire(null));
        Assert.Equal(WarningCode.UnknownCode, WarningCodeMapper.FromWire("SOME_FUTURE_CODE"));
        Assert.Equal(WarningCode.UnknownCode, WarningCodeMapper.FromWire(""));
    }

    [Fact]
    public void RoundTrip_AllKnownCodes()
    {
        foreach (var code in Enum.GetValues<WarningCode>().Where(c => c != WarningCode.UnknownCode))
        {
            Assert.Equal(code, WarningCodeMapper.FromWire(WarningCodeMapper.ToWire(code)));
        }
    }
}
using EFCore.DataClassification.Models;
using Xunit;

namespace EFCore.DataClassification.Tests.Models;

/// <summary>
/// Tests for SensitivityRank enum
/// </summary>
public class SensitivityRankTests
{
    [Fact]
    public void SensitivityRank_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)SensitivityRank.None);
        Assert.Equal(1, (int)SensitivityRank.Low);
        Assert.Equal(2, (int)SensitivityRank.Medium);
        Assert.Equal(3, (int)SensitivityRank.High);
        Assert.Equal(4, (int)SensitivityRank.Critical);
    }

    [Fact]
    public void SensitivityRank_ToString_ReturnsName()
    {
        // Assert
        Assert.Equal("None", SensitivityRank.None.ToString());
        Assert.Equal("Low", SensitivityRank.Low.ToString());
        Assert.Equal("Medium", SensitivityRank.Medium.ToString());
        Assert.Equal("High", SensitivityRank.High.ToString());
        Assert.Equal("Critical", SensitivityRank.Critical.ToString());
    }

    [Fact]
    public void SensitivityRank_CanBeCompared()
    {
        // Assert
        Assert.True(SensitivityRank.Low < SensitivityRank.High);
        Assert.True(SensitivityRank.High > SensitivityRank.Medium);
        Assert.True(SensitivityRank.Critical > SensitivityRank.None);
        Assert.Equal(SensitivityRank.Medium, SensitivityRank.Medium);
    }
}









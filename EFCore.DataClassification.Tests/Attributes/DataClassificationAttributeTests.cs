using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.Reflection;
using Xunit;

namespace EFCore.DataClassification.Tests.Attributes;

/// <summary>
/// Tests for DataClassificationAttribute
/// </summary>
public class DataClassificationAttributeTests
{
    private class TestClass
    {
        [DataClassification("Confidential", "Email", SensitivityRank.High)]
        public string Email { get; set; } = "";

        [DataClassification("Public", "Name", SensitivityRank.None)]
        public string Name { get; set; } = "";

        public string NoAttribute { get; set; } = "";
    }

    [Fact]
    public void Attribute_SetsProperties()
    {
        // Arrange
        var attribute = new DataClassificationAttribute("Test Label", "Test Type", SensitivityRank.Medium);

        // Assert
        Assert.Equal("Test Label", attribute.Label);
        Assert.Equal("Test Type", attribute.InformationType);
        Assert.Equal(SensitivityRank.Medium, attribute.Rank);
    }

    [Fact]
    public void Attribute_CanBeAppliedToProperty()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Email))!;

        // Act
        var attribute = propertyInfo.GetCustomAttribute<DataClassificationAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Confidential", attribute.Label);
        Assert.Equal("Email", attribute.InformationType);
        Assert.Equal(SensitivityRank.High, attribute.Rank);
    }

    [Fact]
    public void Attribute_WithNoneRank_IsValid()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Name))!;

        // Act
        var attribute = propertyInfo.GetCustomAttribute<DataClassificationAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Public", attribute.Label);
        Assert.Equal("Name", attribute.InformationType);
        Assert.Equal(SensitivityRank.None, attribute.Rank);
    }

    [Fact]
    public void Attribute_NotPresent_ReturnsNull()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.NoAttribute))!;

        // Act
        var attribute = propertyInfo.GetCustomAttribute<DataClassificationAttribute>();

        // Assert
        Assert.Null(attribute);
    }

    [Fact]
    public void Attribute_AllowsOnlyOneInstance()
    {
        // Arrange
        var attributeUsage = typeof(DataClassificationAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.False(attributeUsage.AllowMultiple);
        Assert.Equal(AttributeTargets.Property, attributeUsage.ValidOn);
    }
}









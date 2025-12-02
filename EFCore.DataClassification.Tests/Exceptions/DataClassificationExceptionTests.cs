using EFCore.DataClassification.Exceptions;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EFCore.DataClassification.Tests.Exceptions;

public class DataClassificationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var exception = new DataClassificationException("Test error message");

        // Assert
        Assert.Equal("Test error message", exception.Message);
        Assert.Null(exception.Property);
    }

    [Fact]
    public void Constructor_WithPropertyAndMessage_SetsPropertyAndMessage()
    {
        // Arrange
        IProperty? mockProperty = null; // In real scenario, you'd use a real property from ModelBuilder

        // Act
        var exception = new DataClassificationException(mockProperty!, "Property-specific error");

        // Assert
        Assert.Equal("Property-specific error", exception.Message);
        Assert.Equal(mockProperty, exception.Property);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DataClassificationException("Outer error", innerException);

        // Assert
        Assert.Equal("Outer error", exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_Default_CreatesException()
    {
        // Act
        var exception = new DataClassificationException();

        // Assert
        Assert.NotNull(exception);
        Assert.Null(exception.Property);
    }
}









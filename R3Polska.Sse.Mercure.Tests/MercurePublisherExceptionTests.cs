using Shouldly;

namespace R3Polska.Sse.Mercure.Tests;

public class MercurePublisherExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        const string expectedMessage = "Test error message";

        // Act
        var exception = new MercurePublisherException(expectedMessage);

        // Assert
        exception.Message.ShouldBe(expectedMessage);
        exception.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBothProperties()
    {
        // Arrange
        const string expectedMessage = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new MercurePublisherException(expectedMessage, innerException);

        // Assert
        exception.Message.ShouldBe(expectedMessage);
        exception.InnerException.ShouldBe(innerException);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_SetsEmptyMessage()
    {
        // Arrange & Act
        var exception = new MercurePublisherException(string.Empty);

        // Assert
        exception.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Exception_IsOfTypeException()
    {
        // Arrange & Act
        var exception = new MercurePublisherException("Test");

        // Assert
        exception.ShouldBeAssignableTo<Exception>();
    }
}

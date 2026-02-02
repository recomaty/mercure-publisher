using System.ComponentModel.DataAnnotations;
using Shouldly;

namespace R3Polska.Sse.Mercure.Tests;

public class MercurePublisherOptionsTests
{
    [Fact]
    public void Validate_ValidHostAndToken_ReturnsSuccess()
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = "https://mercure.example.com",
            Token = "valid-jwt-token"
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.ShouldBeEmpty();
    }

    [Fact]
    public void Validate_ValidHostWithPath_ReturnsSuccess()
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = "https://mercure.example.com/hub",
            Token = "valid-jwt-token"
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyHost_ReturnsFailure(string host)
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = host,
            Token = "valid-jwt-token"
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.ShouldNotBeEmpty();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Host"));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("just-some-text")]
    [InlineData("://missing-scheme")]
    [InlineData("http:/missing-slash")]
    public void Validate_InvalidHostUrl_ReturnsFailure(string host)
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = host,
            Token = "valid-jwt-token"
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.ShouldNotBeEmpty();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Host"));
    }

    [Theory]
    [InlineData("")]
    public void Validate_EmptyToken_ReturnsFailure(string token)
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = "https://mercure.example.com",
            Token = token
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.ShouldNotBeEmpty();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Token"));
    }

    [Fact]
    public void Validate_BothHostAndTokenInvalid_ReturnsMultipleFailures()
    {
        // Arrange
        var options = new MercurePublisherOptions
        {
            Host = "invalid-url",
            Token = ""
        };

        // Act
        var validationResults = ValidateOptions(options);

        // Assert
        validationResults.Count.ShouldBeGreaterThanOrEqualTo(2);
        validationResults.ShouldContain(r => r.MemberNames.Contains("Host"));
        validationResults.ShouldContain(r => r.MemberNames.Contains("Token"));
    }

    private static List<ValidationResult> ValidateOptions(MercurePublisherOptions options)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(options);
        Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}

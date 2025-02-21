using System.Text;
using System.Text.Json;
using FluentAssertions;

public class QuoteTokenServiceTests
{
    private readonly QuoteTokenService _service;
    private readonly string _secretKey = "test_secret";

    public QuoteTokenServiceTests()
    {
        _service = new QuoteTokenService(_secretKey);
    }

    [Fact]
    public void GenerateQuoteToken_Should_Return_Valid_Token()
    {
        // Arrange
        var payload = new QuoteTokenPayload { CustomerId = 123, Items = new List<RequestedItem> { new() { ItemId = 33, Quantity = 2 }, new() { ItemId = 44, Quantity = 1 } }, TotalPrice = 840, ExpiresAt = DateTime.UtcNow };

        // Act
        string token = _service.GenerateQuoteToken(payload);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().Contain(".");

        var parts = token.Split('.');
        parts.Should().HaveCount(2); // Ensure it has payload and signature

        var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
        decodedPayload.Should().Be(JsonSerializer.Serialize(payload));
    }
}
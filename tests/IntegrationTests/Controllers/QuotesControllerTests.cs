using System.Net.Http.Json;
using FluentAssertions;

public class QuotesControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;

    public QuotesControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
    }
    [Fact]
    public async Task CreateQuote_ShouldReturnQuoteResponse()
    {
        // Arrange
        var request = new CustomerItemsRequest
        {
            CustomerId = 1,
            Items = Fixtures.itemRequests
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Quotes, request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QuoteResponse>();
        result.Should().NotBeNull();
        result!.QuoteId.Should().BeGreaterThan(0);
        result.QuoteToken.Should().NotBeNullOrWhiteSpace();

        // Verify the created quote in the database
        var repo = _factory.GetRepoFromServices<QuotesRepository>();
        var createdQuote = await repo.GetQuoteById(result.QuoteId);
        createdQuote.Should().NotBeNull();
        createdQuote!.CustomerId.Should().Be(request.CustomerId);
        createdQuote.Price.Should().Be(result.QuoteTokenPayload.TotalPrice);

        // Verify the created quote items in the database
        var quotesItemsRepo = _factory.GetRepoFromServices<QuotesItemsRepository>();
        var createdQuoteItems = await quotesItemsRepo.GetQuoteItemsByQuoteId(result.QuoteId);
        createdQuoteItems.Should().NotBeNull();
        createdQuoteItems.Should().HaveCount(request.Items.Count);

        foreach (var item in request.Items)
        {
            var matchingItem = createdQuoteItems.Should().ContainSingle(qi => qi.ItemId == item.ItemId && qi.Quantity == item.Quantity);
            matchingItem.Which.QuoteId.Should().Be(result.QuoteId);
            matchingItem.Which.TotalPrice.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetQuote_ShouldReturnQuote_WhenExists()
    {
        // Arrange
        var request = new CustomerItemsRequest
        {
            CustomerId = 1,
            Items = new List<ItemRequest>
            {
                new() { ItemId = Fixtures.itemsFixturesIds[0], Quantity = 1 }
            }
        };
        var repo = _factory.GetRepoFromServices<QuotesRepository>();
        var expiry = DateTime.UtcNow.AddMinutes(5);
        var price = Fixtures.itemsFixtures[0].Price;
        int quoteId = await repo.CreateQuote(request.CustomerId, price, expiry);

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Quotes + quoteId);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Quote>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(quoteId);
    }

    [Fact]
    public async Task GetQuote_ShouldReturnNotFound_WhenQuoteDoesNotExist()
    {
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Quotes + "9999999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UseQuote_ShouldMarkQuoteAsUsed()
    {
        // Arrange
        var request = new CustomerItemsRequest
        {
            CustomerId = 1,
            Items = new List<ItemRequest>
            {
                new() { ItemId = Fixtures.itemsFixturesIds[0], Quantity = 1 }
            }
        };
        var repo = _factory.GetRepoFromServices<QuotesRepository>();
        var expiry = DateTime.UtcNow.AddMinutes(5);
        var price = Fixtures.itemsFixtures[0].Price;
        int quoteId = await repo.CreateQuote(request.CustomerId, price, expiry);

        // Act
        var response = await _factory.Client.PatchAsync(HttpHelper.Urls.UseQuote + quoteId, null);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedQuote = await repo.GetQuoteById(quoteId);
        updatedQuote.Should().NotBeNull();
        updatedQuote!.IsUsed.Should().BeTrue();
    }
}




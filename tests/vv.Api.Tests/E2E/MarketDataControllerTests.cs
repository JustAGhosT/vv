using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using vv.Application.Services;
using vv.Domain.Models;
using vv.Domain.Repositories;
using Xunit;

namespace vv.Api.Tests.E2E
{
    public class MarketDataControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MarketDataControllerTests(WebApplicationFactory<Program> factory)
        {
            // Configure the factory with test services
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real MarketDataService and repository
                    var serviceDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMarketDataService));

                    if (serviceDescriptor != null)
                    {
                        services.Remove(serviceDescriptor);
                    }

                    // Register mock services for testing
                    var mockService = new Mock<IMarketDataService>();

                    // Setup mock responses
                    var testData = new FxSpotPriceData
                    {
                        AssetId = "eurusd",
                        DisplayAssetId = "EUR/USD",
                        AssetClass = "fx",
                        DataType = "price.spot",
                        Region = "global",
                        DocumentType = "official",
                        Price = 1.05m,
                        AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                        SchemaVersion = "1.0"
                    };

                    mockService.Setup(s => s.GetLatestMarketDataAsync(
                        It.IsAny<string>(),
                        "fx",
                        "eurusd",
                        It.IsAny<string>(),
                        It.IsAny<DateOnly>(),
                        It.IsAny<string>()))
                        .ReturnsAsync(testData);

                    mockService.Setup(s => s.GetLatestMarketDataAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        "invalidasset",
                        It.IsAny<string>(),
                        It.IsAny<DateOnly>(),
                        It.IsAny<string>()))
                        .ReturnsAsync((FxSpotPriceData)null);

                    mockService.Setup(s => s.CreateMarketDataAsync(It.IsAny<FxSpotPriceData>()))
                        .ReturnsAsync("test-id-123");

                    mockService.Setup(s => s.QueryAsync(
                            It.IsAny<Expression<Func<FxSpotPriceData, bool>>>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<FxSpotPriceData> { testData });

                    services.AddTransient(_ => mockService.Object);
                });
            });
        }

        [Fact]
        public async Task GetLatestMarketData_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/marketdata/fx/eurusd/latest");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");
        }

        [Fact]
        public async Task GetLatestMarketData_ReturnsExpectedData()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/marketdata/fx/eurusd/latest");
            var result = await response.Content.ReadFromJsonAsync<FxSpotPriceData>();

            // Assert
            result.Should().NotBeNull();
            result.AssetId.Should().Be("eurusd");
            result.Price.Should().Be(1.05m);
        }

        [Fact]
        public async Task GetLatestMarketData_WithInvalidAsset_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/marketdata/fx/invalidasset/latest");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateMarketData_ReturnsCreatedResultWithLocation()
        {
            // Arrange
            var client = _factory.CreateClient();
            var data = new FxSpotPriceData
            {
                AssetId = "eurusd",
                DisplayAssetId = "EUR/USD",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                Price = 1.05m,
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                SchemaVersion = "1.0"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/marketdata",
                new { Data = data });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var result = await response.Content.ReadFromJsonAsync<string>();
            result.Should().Be("test-id-123");
        }

        [Fact]
        public async Task QueryMarketData_ReturnsMatchingItems()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/marketdata/query?AssetClass=fx&AssetId=eurusd");

            // Assert
            response.EnsureSuccessStatusCode();
            var results = await response.Content.ReadFromJsonAsync<List<FxSpotPriceData>>();

            results.Should().NotBeNull();
            results.Should().HaveCount(1);
            results[0].AssetId.Should().Be("eurusd");
        }
    }
}
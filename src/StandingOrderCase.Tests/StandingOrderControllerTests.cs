using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Records;

namespace StandingOrderCase.Tests;

public class StandingOrderControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    private readonly HttpClient _client;

    public StandingOrderControllerTests(WebApplicationFactory<Program> factory)
    {
        // Arrange
        _client = factory
            .WithWebHostBuilder(builder =>
                builder.UseEnvironment("IntegrationTest"))
            .CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsStandingOrder_WhenExistingStandingOrderIdIsPassed()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            250);

        var postResponse = await _client.PostAsJsonAsync("/standing-orders", model);
        postResponse.EnsureSuccessStatusCode();
        var createdStandingOrder = await postResponse.Content.ReadFromJsonAsync<CreateStandingOrderResult>();

        // Act
        var response = await _client.GetAsync($"/standing-orders/{createdStandingOrder.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var standingOrder = await response.Content.ReadFromJsonAsync<GetStandingOrder>(_jsonSerializerOptions);
        Assert.Equal(createdStandingOrder.Id, standingOrder.Id);
    }

    [Fact]
    public async Task GetByUserId_ReturnsStandingOrders_WhenExistingUserIdIsPassed()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            250);
        var postResponse = await _client.PostAsJsonAsync("/standing-orders", model);
        postResponse.EnsureSuccessStatusCode();
        var createdStandingOrder = await postResponse.Content.ReadFromJsonAsync<CreateStandingOrderResult>();

        // Act
        var response = await _client.GetAsync($"standing-orders?userId={userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Deserialize and verify the returned standing orders
        var standingOrder = await response.Content.ReadFromJsonAsync<GetStandingOrder>(_jsonSerializerOptions);
        Assert.Equal(createdStandingOrder.Id, standingOrder.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenInvalidStandingOrderIdIsPassed()
    {
        // Arrange
        var standingOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/standing-orders/{standingOrderId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_ReturnsNotifications_WhenExistingStandingOrderIdIsPassed()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            new[] { NotificationTypeEnum.Email },
            250);
        var postResponse = await _client.PostAsJsonAsync("/standing-orders", model, _jsonSerializerOptions);
        postResponse.EnsureSuccessStatusCode();
        
        var createdStandingOrder = await postResponse.Content.ReadFromJsonAsync<CreateStandingOrderResult>(_jsonSerializerOptions);

        // Act
        var response = await _client.GetAsync($"/notifications?standingOrderId={createdStandingOrder.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Deserialize and verify the returned notifications
        var notifications = await response.Content.ReadFromJsonAsync<List<GetNotification>>(_jsonSerializerOptions);
        Assert.Contains(notifications, n => n.StandingOrderId == createdStandingOrder.Id);
    }

    [Fact]
    public async Task GetNotifications_ReturnsNotFound_WhenInvalidStandingOrderIdIsPassed()
    {
        // Arrange
        var standingOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/standing-orders/{standingOrderId}/notifications");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsSuccess_WhenValidModelIsPassed()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            250);

        // Act
        var response = await _client.PostAsJsonAsync("/standing-orders", model);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var standingOrder = await response.Content.ReadFromJsonAsync<CreateStandingOrderResult>();
        Assert.NotEqual(Guid.Empty, standingOrder.Id);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenExecutionDateIsInThePast()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), // execution date in the past
            Array.Empty<NotificationTypeEnum>(),
            250);

        // Act
        var response = await _client.PostAsJsonAsync("/standing-orders", model);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationError[]>();
        Assert.Contains(error, e => e.PropertyName == "ExecutionDate");
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenExecutionDateHasInvalidDayOfMonth()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                31)), // invalid day of month (31st)
            Array.Empty<NotificationTypeEnum>(),
            250);

        // Act
        var response = await _client.PostAsJsonAsync("/standing-orders", model);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationError[]>();
        Assert.Contains(error, e => e.PropertyName == "ExecutionDate.Day");
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenAmountIsNotInRange()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            99); // amount is too low

        // Act
        var response = await _client.PostAsJsonAsync("/standing-orders", model);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationError[]>();
        Assert.Contains(error, e => e.PropertyName == "Amount");
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenUserAlreadyHasAStandingOrder()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            250);

        // create a standing order for the user
        var postResponse = await _client.PostAsJsonAsync("/standing-orders", model);
        postResponse.EnsureSuccessStatusCode();

        // try to create another standing order for the same user
        var response = await _client.PostAsJsonAsync("/standing-orders", model);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationError[]>();
        Assert.Contains(error, e => e.ErrorMessage == "User already has a standing order");
    }

    [Fact]
    public async Task Cancel_ReturnsOk_WhenValidStandingOrderIdIsPassed()
    {
        // Arrange
        var userId = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe");
        var date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month + 1, 10);
        var model = new CreateStandingOrder(
            userId,
            DateOnly.FromDateTime(date),
            Array.Empty<NotificationTypeEnum>(),
            250);

        var postResponse = await _client.PostAsJsonAsync("/standing-orders", model);
        postResponse.EnsureSuccessStatusCode();
        var createdStandingOrder = await postResponse.Content.ReadFromJsonAsync<CreateStandingOrderResult>();

        // Act
        var response = await _client.DeleteAsync($"/standing-orders/{createdStandingOrder.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
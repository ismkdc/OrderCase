using System.Text.Json.Serialization;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StandingOrderCase.Api.Consumers.Notification;
using StandingOrderCase.Api.HostedServices;
using StandingOrderCase.Api.JsonConverters;
using StandingOrderCase.Api.Models;
using StandingOrderCase.Api.Records;
using StandingOrderCase.Api.Services;
using StandingOrderCase.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
        // serialize DateOnly as strings
        opts.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
});

#region Dependency Injection

//register db context
builder.Services.AddDbContextPool<StandingOrderCaseContext>(
    x => x.UseInMemoryDatabase(nameof(StandingOrderCaseContext))
);

builder.Services.AddScoped<IValidator<CreateStandingOrder>, CreateStandingOrderValidator>();
builder.Services.AddScoped<StandingOrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<OutboxService>();

//register MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendSmsNotificationConsumer>();
    x.AddConsumer<SendEmailNotificationConsumer>();
    x.AddConsumer<SendPushNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
});

//register httpclientfactory
builder.Services.AddHttpClient();

//register hosted service
if (builder.Environment.EnvironmentName != "IntegrationTest")
{
    builder.Services.AddHostedService<OutboxHostedService>();
}

#endregion

var app = builder.Build();

// ensure the database is created
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<StandingOrderCaseContext>();
await dbContext.Database.EnsureCreatedAsync();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();

public partial class Program
{
}
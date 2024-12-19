
using HotelCustomerPost.Entities;
using HotelCustomerPost.RabbitMq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:4951");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services
    .AddCors(options =>
        options.AddPolicy(
            "Customer",
            policy =>
                policy
                    .WithOrigins("https://localhost:7151", "https://localhost:7173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
        )
    );

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Customer");

// Endpoint f�r pots
app.MapPost("/customers", async (Customer customer, RabbitMqService rabbitMqService) =>
{
    try
    {
        var jsonCustomer = JsonSerializer.Serialize(customer);
        rabbitMqService.PublishMessage(jsonCustomer);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.Run();
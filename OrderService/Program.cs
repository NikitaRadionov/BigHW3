using Microsoft.EntityFrameworkCore;
using OrdersService.Database;
using OrderService.UseCases.CreateOrder;
using OrderService.UseCases.GetUserOrders;
using OrderService.UseCases.GetOrderById;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<MigrationRunner>();
builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.AddHostedService<PaymentStatusConsumerService>();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<ICreateOrderService, CreateOrderService>();
builder.Services.AddScoped<IGetUserOrdersService, GetUserOrdersService>();
builder.Services.AddScoped<IGetOrderByIdService, GetOrderByIdService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
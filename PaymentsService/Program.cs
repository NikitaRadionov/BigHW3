using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Messaging;
using PaymentsService.UseCases.CreateAccount;
using PaymentsService.UseCases.Deposit;
using PaymentsService.UseCases.GetBalance;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHostedService<MigrationRunner>();
builder.Services.AddHostedService<OrderMessageConsumer>();
builder.Services.AddHostedService<InboxProcessorService>();
builder.Services.AddHostedService<OutboxPublisherService>();

builder.Services.AddScoped<ICreateAccountService, CreateAccountService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IGetBalanceService, GetBalanceService>();

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
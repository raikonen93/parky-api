using Parky.Application.Interfaces;
using Parky.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (simple console)
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Fatal)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Fatal)
        .WriteTo.File(
            path: "logs/consumer.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 3,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKafkaCommandHandler, KafkaCommandHandler>();
builder.Services.AddHostedService<KafkaBackgroundConsumer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

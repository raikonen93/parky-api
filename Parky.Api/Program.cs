using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Parky.Application.Mapping;
using Parky.Infrastructure.Context;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

//SeriLog Settings+Coreletion Id
builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation(options => options.Headers.Add("X-Correlation-ID"));

builder.Host.UseSerilog((context, config) => config
    .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationIdHeader("X-Correlation-ID"));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//Adding swagger service
builder.Services.AddSwaggerGen();
//Adding health check
builder.Services.AddHealthChecks();

//adding db context
builder.Services.AddDbContext<ParkyDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//add mapping
builder.Services.AddAutoMapper(config => { }, typeof(ParkyMappingProfile));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParkyDbContext>();
    db.Database.Migrate();
}

app.UseHeaderPropagation();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

app.Run();

using LogisticsPartnerHub.Api.Middleware;
using LogisticsPartnerHub.Application.Services;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using LogisticsPartnerHub.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Logistics Partner Hub", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LogisticsPartnerHub.Application.Commands.Partners.CreatePartnerCommand).Assembly));

// Application services
builder.Services.AddScoped<IPayloadTransformer, PayloadTransformerService>();

// Infrastructure (EF Core, Repositories, HttpClients, Background Jobs)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=logistics_partner_hub;Username=postgres;Password=postgres";
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

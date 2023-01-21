using Bookings.Payments;
using Bookings.Payments.Domain;
using Bookings.Payments.Infrastructure;
using Eventuous;
using Eventuous.AspNetCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

TypeMap.RegisterKnownEventTypes();
Logging.ConfigureLog();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-7.0&tabs=visual-studio
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "(Bookings) Payment API",
        Description = "An ASP.NET Core Web API for managing (Bookings) Payments ",
        //TermsOfService = new Uri("https://example.com/terms"),
        //Contact = new OpenApiContact
        //{
        //    Name = "Example Contact",
        //    Url = new Uri("https://example.com/contact")
        //},
        //License = new OpenApiLicense
        //{
        //    Name = "Example License",
        //    Url = new Uri("https://example.com/license")
        //}
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// OpenTelemetry instrumentation must be added before adding Eventuous services
builder.Services.AddOpenTelemetry();

builder.Services.AddServices(builder.Configuration);
builder.Host.UseSerilog();

var app = builder.Build();
app.AddEventuousLogs();

app.UseSwagger();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Here we discover commands by their annotations
// app.MapDiscoveredCommands();
app.MapDiscoveredCommands<Payment>();

app.UseSwaggerUI();

app.Run();
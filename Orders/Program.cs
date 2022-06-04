
using Eventuous;
using Eventuous.AspNetCore;
using Eventuous.Diagnostics.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Orders;
using Orders.Domain.Orders;
using Serilog;
using Serilog.Events;


/*
 * API is running on port 44320 (if not changed in launchsettings.json)
 */

//register event types from assembly; EventTypes must be decorated with attribute, ex: [EventType("V1.OrderAdded")]
TypeMap.RegisterKnownEventTypes(typeof(OrderEvents.V1.OrderAdded).Assembly);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Grpc", LogEventLevel.Information)
    .MinimumLevel.Override("Grpc.Net.Client.Internal.GrpcCall", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.Seq("http://localhost:5341") //Serilog needs to be installed for this
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services
    .AddControllers()
    .AddJsonOptions(cfg => cfg.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddOpenTelemetry();
builder.Services.AddEventuous(builder.Configuration); //in Registrations.cs


var app = builder.Build();

app.UseSerilogRequestLogging();
app.AddEventuousLogs();
app.UseSwagger().UseSwaggerUI();
app.MapControllers();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

var factory = app.Services.GetRequiredService<ILoggerFactory>();
var listener = new LoggingEventListener(factory, "OpenTelemetry");

try
{
    app.Run();
    //app.Run("http://*:5020");
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
    listener.Dispose();
}

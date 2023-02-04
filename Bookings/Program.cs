using Bookings;
using Bookings.Domain.Bookings;
using Eventuous;
using Eventuous.AspNetCore;
using Eventuous.Diagnostics.Logging;
using Eventuous.Spyglass;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Bookings.Hubs;
using Microsoft.AspNetCore.Http.Connections;

TypeMap.RegisterKnownEventTypes(typeof(BookingEvents.V1.RoomBooked).Assembly);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
	//.MinimumLevel.Debug()
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
builder.Services.AddSwaggerGen(options =>
{
    //https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-7.0&tabs=visual-studio
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Bookings API",
        Description = "An ASP.NET Core Web API for managing Bookings ",
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

//add SignalR before Eventuous due to DI of IHubContext in HubService inside Eventuous subscribers.
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
    
});


builder.Services.AddOpenTelemetry();
builder.Services.AddEventuous(builder.Configuration);
builder.Services.AddEventuousSpyglass();


builder.Services.Configure<JsonOptions>(options
    => options.SerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
);


/* It's important to note that the UseCors invocation must be placed between UseRouting and UseEndpoints invocations if they are present.*/
//builder.Services.AddCors(options =>
//    options.AddDefaultPolicy(policy =>
//        policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())
//);

//alternative, with policy name and specified origins
builder.Services.AddCors(options =>
{
    /*
     * A note on Signalr and CORS
     * "WebSockets and CORS are not compatible " says Brennan on the signalR team : skipNegotiation will skip the HTTP negotiate that does transport fallback and will only run WebSockets. WebSockets and CORS are not compatible so you end up skipping CORS when using that.
       https://github.com/dotnet/aspnetcore/issues/4457#issuecomment-444738776 -
     */
    options.AddPolicy("AllowCorsPolicyName",
        builder =>
        {
            builder
                .WithOrigins(
                    //assuming frontend runs on port 4200
                    "http://localhost:4200"
                    //"https://*.example.com"
                    //, "*"

                )
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});




var app = builder.Build();

//app.UseSerilogRequestLogging();
app.AddEventuousLogs();
app.UseSwagger().UseSwaggerUI();
app.MapControllers();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapEventuousSpyglass(null);

// With endpoint routing, the CORS middleware must be configured to execute between the calls to UseRouting and UseEndpoints(i.e. app.MapHub)
app.UseCors("AllowCorsPolicyName");

//this hub is used in Angular app found at https://github.com/JimiSweden/AngularExamplesAndSnippets
// in "app/bookings" (bracnh, if not merged to master, bookings-with-eventstore-backend)
app.MapHub<BookingsHub>("/hubs/bookingsHub", options =>
{
    options.Transports = HttpTransportType.WebSockets; //only allow websockets
});


var factory  = app.Services.GetRequiredService<ILoggerFactory>();
var listener = new LoggingEventListener(factory, "OpenTelemetry");

try {
    app.Run();
    //app.Run("http://*:5003");
    //app.Run("http://*:5051");
    
    return 0;
}
catch (Exception e) {
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally {
    Log.CloseAndFlush();
    listener.Dispose();
}

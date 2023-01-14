using System.Text.Json;
using Bookings.Application;
using Bookings.Application.Queries;
using Bookings.Domain;
using Bookings.Domain.Bookings;
using Bookings.Infrastructure;
using Bookings.Integration;
using Eventuous;
using Eventuous.Diagnostics.OpenTelemetry;
using Eventuous.EventStore;
using Eventuous.EventStore.Subscriptions;
using Eventuous.Projections.MongoDB;
using Eventuous.Subscriptions.Registrations;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Bookings;

public static class Registrations {
    public static void AddEventuous(this IServiceCollection services, IConfiguration configuration) {
        DefaultEventSerializer.SetDefaultSerializer(
            new DefaultEventSerializer(
                new JsonSerializerOptions(JsonSerializerDefaults.Web).ConfigureForNodaTime(
                    DateTimeZoneProviders.Tzdb
                )
            )
        );

        // See 'README how to run etc.md' for ESDB notes        
        services.AddEventStoreClient(configuration["EventStore:ConnectionString"]!);
        //Note: if you have login (default) on the Event store database, you need the user and password.
//TODO.. efter merge, till config. 
                //services.AddEventStoreClient("esdb://admin:changeit@localhost:2113?tls=true&tlsVerifyCert=false");
        //services.AddEventStoreClient("esdb://localhost:2113?tls=false");
        services.AddAggregateStore<EsdbEventStore>();


        /* register the command handlers for the Booking Aggregate
         * this is an Eventuous extension.
         */
        services.AddApplicationService<BookingsCommandService, Booking>();

        /* Note: services.methods are delegates, used in the BookingsCommandService as injected.
       * a standard method can't be injected (the whole service would be needed)
       */
        //services.AddSingleton<Services.IsRoomAvailable>((id, period) => new ValueTask<bool>(true)); //a dummy implementation always returning true. 
        services.AddSingleton<Services.IsRoomAvailable>(RoomCheckerService.IsRoomAvailable); //with implementation


        services.AddSingleton<Services.ConvertCurrency>((from, currency) => new Money(from.Amount * 2, currency));

        services.AddSingleton(Mongo.ConfigureMongo(configuration));

        services.AddCheckpointStore<MongoCheckpointStore>();

        services.AddSubscription<AllStreamSubscription, AllStreamSubscriptionOptions>(
            "BookingsProjections",
            builder => builder
//.Configure(cfg => cfg.ConcurrencyLimit = 2)
                .UseCheckpointStore<MongoCheckpointStore>()
                .AddEventHandler<BookingStateProjection>()
                .AddEventHandler<MyBookingsProjection>()
                .AddEventHandler<StatisticsOfBookingsProjection>()
                //TODO: add projection holding the available rooms and booked dates
                .WithPartitioningByStream(2)
        );

        services.AddSubscription<StreamPersistentSubscription, StreamPersistentSubscriptionOptions>(
            "PaymentIntegration",
            builder => builder
                .Configure(x => x.StreamName = PaymentsIntegrationHandler.Stream)
                .AddEventHandler<PaymentsIntegrationHandler>()
        );
    }

    public static void AddOpenTelemetry(this IServiceCollection services) {
        var otelEnabled = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null;

        services.AddOpenTelemetryMetrics(
            builder => {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("bookings"))
                    .AddAspNetCoreInstrumentation()
                    .AddEventuous()
                    .AddEventuousSubscriptions()
                    .AddPrometheusExporter();
                if (otelEnabled) builder.AddOtlpExporter();
            }
        );

        services.AddOpenTelemetryTracing(
            builder => {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddEventuousTracing()
                    .AddMongoDBInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("bookings"))
                    .SetSampler(new AlwaysOnSampler());

                if (otelEnabled)
                    builder.AddOtlpExporter();
                else
                    builder.AddZipkinExporter();
            }
        );
    }
}

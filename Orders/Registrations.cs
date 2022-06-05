using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Eventuous;
using Eventuous.EventStore;
using Eventuous.EventStore.Subscriptions;
using Eventuous.Projections.MongoDB;
using Eventuous.Subscriptions.Registrations;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Orders.Application;
using Orders.Domain;
using Orders.Domain.Orders;
using Orders.Infrastructure;

namespace Orders;

    public static class Registrations
    {

    public static void AddEventuous(this IServiceCollection services, IConfiguration configuration)
    {
        DefaultEventSerializer.SetDefaultSerializer(
            new DefaultEventSerializer(
                new JsonSerializerOptions(JsonSerializerDefaults.Web).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
            )
        );

        // See 'README how to run etc.md' for ESDB notes        
        //Note: if you have login (default) on the Event store database, you need the user and password.
        services.AddEventStoreClient(configuration["EventStore:ConnectionString"]);
        services.AddAggregateStore<EsdbEventStore>();


        /* register the command handlers for the Order Aggregate
         * this is an Eventuous extension.
         */
        services.AddApplicationService<OrdersCommandService, Order>();

        /* Note: services.methods are delegates, used in the OrdersCommandService as injected.
       * - a standard method can't be injected (the whole service would be needed to inject)
       */
        services.AddSingleton<Services.IsProductAvailable>(ProductAvailabilityService.IsProductAvailable);
        services.AddSingleton<Services.MaxProductsOrderable>(ProductAvailabilityService.MaxProductsOrderable);


        //todo: if wanted.. real conversion , this is just a dummy
        //services.AddSingleton<Services.ConvertCurrency>((from, currency) => new Money(from.Amount * 2, currency));


        //AllStreamSubscription is a Catch - up subscription for EventStoreDB, using the $all global stream
        //services.AddSubscription<AllStreamSubscription, AllStreamSubscriptionOptions>(
        //    "OrdersProjections",
        //    builder => builder
        //        .Configure(cfg => cfg.ConcurrencyLimit = 2)
        //        .AddEventHandler<BookingStateProjection>()
        //        .AddEventHandler<MyBookingsProjection>()
        //        //TODO: add projection holding the available rooms and booked dates
        //        .WithPartitioningByStream(2)
        //);



        //todo: enable mongo db for projections.. 
        //services.AddSingleton(Mongo.ConfigureMongo(configuration)); 

        //services.AddCheckpointStore<MongoCheckpointStore>(); //eventuous extension

        /* subscriptions below are "listening" to  EventStoreDB $all global stream
         * and updates the MongoDb projection views (read models) 
         */
        //AllStreamSubscription is a Catch-up subscription for EventStoreDB, using the $all global stream
        //services.AddSubscription<AllStreamSubscription, AllStreamSubscriptionOptions>(
        //    "BookingsProjections",
        //    builder => builder
        //        .Configure(cfg => cfg.ConcurrencyLimit = 2)
        //        .AddEventHandler<BookingStateProjection>()
        //        .AddEventHandler<MyBookingsProjection>()
        //        //TODO: add projection holding the available rooms and booked dates
        //        .WithPartitioningByStream(2)
        //);

        //services.AddSubscription<StreamSubscription, StreamSubscriptionOptions>(
        //    "PaymentIntegration",
        //    builder => builder
        //        .Configure(x => x.StreamName = PaymentsIntegrationHandler.Stream)
        //        .AddEventHandler<PaymentsIntegrationHandler>()
        //);
    }
}

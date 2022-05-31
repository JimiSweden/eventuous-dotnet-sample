using System.Diagnostics;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Bookings.Payments.Infrastructure;

public static class Mongo
{
    public static IMongoDatabase ConfigureMongo(IConfiguration configuration)
    {

        try
        {
            var settings = MongoClientSettings.FromConnectionString(configuration["Mongo:ConnectionString"]);
            settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            return new MongoClient(settings).GetDatabase(configuration["Mongo:Database"]);
        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
    }
}
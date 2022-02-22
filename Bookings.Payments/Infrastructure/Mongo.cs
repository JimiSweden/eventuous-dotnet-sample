using MongoDB.Driver;
using System.Diagnostics;

namespace Bookings.Payments.Infrastructure;

public static class Mongo {
    public static IMongoDatabase ConfigureMongo() {

        try
        {

        
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
        //var settings = MongoClientSettings.FromConnectionString("mongodb://mongoadmin:secret@localhost:27017");
        return new MongoClient(settings).GetDatabase("payments");

        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
    }
}
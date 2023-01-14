using Eventuous.Projections.MongoDB.Tools;

namespace Bookings.Application.Queries;

/// <summary>
/// Note: the class name, without the suffix Document, will be the name used for the MongoDB Collection
/// in this example "StatisticsOfBookings"
/// (*by default , todo : how to set the name maually?)
/// </summary>
public record StatisticsOfBookingsDocument : ProjectedDocument
{
    public StatisticsOfBookingsDocument(string id) : base(id) { }
    public List<GuestWithBookings> Guests { get; set; }

    public record GuestWithBookings(string GuestId);
}
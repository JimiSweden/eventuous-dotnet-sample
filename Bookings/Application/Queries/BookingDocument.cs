using Eventuous.Projections.MongoDB.Tools;
using NodaTime;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Bookings.Application.Queries;

/// <summary>
/// represents the document contract in MongoDb <br/>
/// as you see the types are of basic ones
/// 
/// </summary>
public record BookingDocument : ProjectedDocument {
    public BookingDocument(string id) : base(id) { }

    public string GuestId { get; init; } = null!;
    public string RoomId  { get; init; } = null!;
    public LocalDate CheckInDate  { get; init; }
    public LocalDate CheckOutDate { get; init; }
    public float     BookingPrice { get; init; }
    public float     PaidAmount   { get; init; }
    public float     Outstanding  { get; init; }
    public bool      Paid         { get; init; }
}
namespace Bookings.Domain;

public static class Services
{

    /// <summary>
    /// Note: this delegate has an example of a dummy implementation in Registrations  (commented out)
    /// services.AddSingleton<Services.IsRoomAvailable>((id, period) => new ValueTask<bool>(true)); //a dummy implementation always returning true. 
    /// 
    /// and an example implementing the RoomCheckerService.IsRoomAvailable found here <see cref="RoomCheckerService"/>
    /// services.AddSingleton<Services.IsRoomAvailable>(RoomCheckerService.IsRoomAvailable); 
    /// 
    /// 
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="period"></param>
    /// <returns></returns>
    public delegate ValueTask<bool> IsRoomAvailable(RoomId roomId, StayPeriod period);

    public delegate Money ConvertCurrency(Money from, string targetCurrency);
}

public class RoomCheckerService
{
    public static ValueTask<bool> IsRoomAvailable(RoomId roomId, StayPeriod period)
    {
        //add logic to validate if the room is booked during the period... 
        //room 13 is always booked :)
        return roomId == new RoomId("13") ? new ValueTask<bool>(false) : new ValueTask<bool>(true);
    }
}
namespace Bookings.Hubs;

/// <summary>
/// Keeping track of users connected to the signalR hub
/// </summary>
public class UserStateService
{
    public List<UserInSignalR> AllLoggedInUsers { get; set; } = new();
    public List<string> ConnectedClients { get; set; } = new();

}
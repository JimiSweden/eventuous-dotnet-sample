namespace Bookings.Hubs;

public class UserInSignalR
{
    public UserInSignalR(string name, string contextConnectionId)
    {
        Name = name;
        ContextConnectionId = contextConnectionId;
    }

    public string Name { get; set; }
    public string ContextConnectionId { get; set; }
}
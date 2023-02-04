using Microsoft.AspNetCore.SignalR;

#pragma warning disable CS1591
namespace Bookings.Hubs;

public static class HubConnectionMethods
{
    public static class HubMethods
    {
        /// <summary>
        /// string fromUser, string message
        /// </summary>
        public static string SendMessage => "SendMessage";

        /// <summary>
        /// string [] toUsers, string fromUser, string message
        /// </summary>
        public static string SendMessageToUsers => "SendMessageToUsers";
    }

    public static class UiClientMethods
    {
        /// <summary>
        /// string fromUser, string message
        /// </summary>
        public static string ReceiveMessage => "ReceiveMessage";
        public static string ClientConnected => "ClientConnected";
        public static string ClientDisconnected => "ClientDisconnected";
        public static string RoomBooked => "RoomBooked";
        public static string BookingChanged => "BookingChanged";
    }
}

//public class BookingsHub : Hub<IBookingsHub>
public class BookingsHub : Hub
{
    private UserStateService _userStateService;

    public BookingsHub(UserStateService userStateService)
    {
        _userStateService = userStateService;
    }

    public override Task OnConnectedAsync()
    {
        _userStateService.ConnectedClients.Add(Context.ConnectionId);


        Broadcast(HubConnectionMethods.UiClientMethods.ClientConnected, "hub context - broadcast",
            $"{HubConnectionMethods.UiClientMethods.ClientConnected}: {Context.ConnectionId}");

        //SendMessageToAllAkaBroadcast("hub context to All",
        //    $"{HubConnectionMethods.UiClientMethods.ClientConnected}: {Context.ConnectionId}");


        AddUserToAllLoggedIn("jimi").ConfigureAwait(false);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _userStateService.ConnectedClients.Remove(Context.ConnectionId);
        Broadcast(HubConnectionMethods.UiClientMethods.ClientDisconnected, "hub context",
            $"{HubConnectionMethods.UiClientMethods.ClientDisconnected}: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// on signing in
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task AddUserToAllLoggedIn(string userName)
    {
        var userId = Context.ConnectionId;
        await AddUserToPersonalGroup(userName);
        //cleanup n add
        _userStateService.AllLoggedInUsers.RemoveAll(x => x.Name == userName);
        _userStateService.AllLoggedInUsers.Add(new UserInSignalR(userName, Context.ConnectionId));

        await Groups.AddToGroupAsync(Context.ConnectionId, "AllLoggedIn");

        await Clients.Group("AllLoggedIn").SendAsync("LoggedIn", $"user {userName}, ConnectionId : {Context.ConnectionId} has joined the group AllLoggedIn.");
    }




    /// <summary>
    /// on signing out or dropping connection.
    /// NOTE: user context will be cleaned up from hub context (groups etc),
    /// but we need to remove disconnected users from our UserStateService
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task RemoveUserFromAllLoggedIn(string userName)
    {
        if (string.IsNullOrEmpty(userName)) return;


        var userId = Context.ConnectionId;
        await RemoveFromGroup(userName);

        var sessions = _userStateService.AllLoggedInUsers.Where(x => x.Name == userName);

        foreach (var session in sessions)
        {
            await Groups.RemoveFromGroupAsync(session.ContextConnectionId, "AllLoggedIn");

        }

        _userStateService.AllLoggedInUsers.RemoveAll(x => x.Name == userName);

        await Clients.Group("AllLoggedIn").SendAsync("LoggedIn", $"user {userName}, ConnectionId : {Context.ConnectionId} has been removed from the group AllLoggedIn.");
    }


    /// <summary>
    /// Endast för enskilt användarnamn - 
    /// Notera att Grupper och Enskild användare behandlas på samma sätt.
    /// Användaren läggs som enda person i grupp för att ha "individuell postlåda".
    /// Om användaren har flera fönster eller tabbar kommer denne att ha flera ConnectionId, samtliga kopplade till samma "egna" grupp (postlåda)
    /// (För grupper läggs flera användare i samma - identitet för användare är alltid "ConnectionId")
    /// //https://learn.microsoft.com/en-us/aspnet/core/signalr/groups?view=aspnetcore-7.0
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task AddUserToPersonalGroup(string userName)
    {
        if (string.IsNullOrEmpty(userName)) return;

        var userId = Context.ConnectionId;

        //notera: flera ConnectionId kan finnas för samma userName. rensning av connectionId tas om hand av signalR.
        await Groups.AddToGroupAsync(Context.ConnectionId, userName);

        await Clients.Group(userName).SendAsync("Send", $"user {userName}, ConnectionId : {Context.ConnectionId} is registered (as group {userName}).");
    }



    public async Task AddToGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    }


    public async Task SendMessageToAllAkaBroadcast(string fromUser, string message)
    {
        await Clients.All.SendAsync(HubConnectionMethods.UiClientMethods.ReceiveMessage, fromUser, message);
    }

    public async Task Broadcast(string methodNamne, string fromUser, string message)
    {
        await Clients.All.SendAsync(methodNamne, fromUser, message);
        await Clients.All.SendAsync(HubConnectionMethods.UiClientMethods.RoomBooked, "bookingId");//todo: ta bort..
    }

    public async Task SendMessageToUsers(IEnumerable<string> toUsers, string fromUser, string message)
    {
        //users registered as group-names
        await Clients.Groups(toUsers).SendAsync(HubConnectionMethods.UiClientMethods.ReceiveMessage, fromUser, message);
        //await Clients.Users(toUsers).SendAsync("ReceiveMessage", fromUser, message);
    }

    public async Task SendMessageToOthers(string fromUser, string message)
    {
        await Clients.Others.SendAsync(HubConnectionMethods.UiClientMethods.ReceiveMessage, fromUser, message);
    }

    public async Task SendMessageToChannel(string groupName, string fromUser, string message)
    {
        await Clients.Group(groupName).SendAsync(HubConnectionMethods.UiClientMethods.ReceiveMessage, fromUser, message);
    }

    public async Task NotifyRoomBooked(string bookingId)
    {
        await Clients.All.SendAsync(HubConnectionMethods.UiClientMethods.RoomBooked, bookingId);
    }

    public async Task NotifyBookingChanged(string bookingId)
    {
        await Clients.All.SendAsync(HubConnectionMethods.UiClientMethods.BookingChanged, bookingId);

    }
}

//public interface IBookingsHub
//{
//    public Task SendAsync(string methodName, string fromUser, string message);
//    public Task RoomBooked(string bookingId);
//    public Task BookingChanged(string bookingid);
//    //bookingCancelled
//}
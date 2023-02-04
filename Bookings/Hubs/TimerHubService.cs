using Microsoft.AspNetCore.SignalR;

namespace Bookings.Hubs;

/// <summary>
/// a simple signalR service with a counter triggered to star and stop from API
/// </summary>
public class TimerHubService
{
    private readonly IHubContext<BookingsHub> _hubContext;

    
    /// <summary>
    /// Note:  When client methods are called from outside of the Hub class,
    /// there's no caller associated with the invocation.
    /// Therefore, there's no access to the ConnectionId, Caller, and Others properties.
    /// (using Groups and "Personal Groups" solves parts of this) 
    /// </summary>
    /// <param name="hubContext"></param>
    public TimerHubService(IHubContext<BookingsHub> hubContext)
    {
        _hubContext = hubContext;
        //StartTimerAndSend();
    }

    public int Counter { get; set; } = 0;
    public Timer MyTimer { get; set; }

    public void StartTimerAndBroadcastMessage()
    {
        MyTimer = new Timer(callback =>
        {
            Counter++;
            _hubContext.Clients.All.SendAsync("timedCounterChanged", $"counter {Counter}").ConfigureAwait(true);

        }, "countertest",1000, 5000); //

        
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }

    public void StopTimer()
    {
        MyTimer.Dispose();
    }

}
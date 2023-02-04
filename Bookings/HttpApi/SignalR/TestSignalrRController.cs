using Bookings.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.HttpApi.SignalR;

[ApiController]
[Route("/testsignalr")]
public class TestSignalrRController
{
    private readonly TimerHubService _timerHubService; //todo. ta bort
    

    public TestSignalrRController(TimerHubService timerHubService)
    {
        _timerHubService = timerHubService;
    }

    [HttpGet]
    [Route("start")]
    public async Task<ActionResult<string>> TriggerSignalrHbContext()
    {
        _timerHubService.StartTimerAndBroadcastMessage();

        return new ActionResult<string>("Timer started, now broadcasting counter updates");
    }


    [HttpGet]
    [Route("stop")]
    public async Task<ActionResult<string>> StopTriggerSignalrHbContext()
    {
        _timerHubService.StopTimer();
        
        return new ActionResult<string>("Timer stopped");
    }
}
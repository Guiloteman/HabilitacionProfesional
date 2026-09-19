using Microsoft.AspNetCore.SignalR;

public class LocationHub : Hub
{
    public async Task SendLocation(string serviceId, double latitude, double longitude)
    {
        await Clients.Group(serviceId).SendAsync("ReceiveLocation", latitude, longitude);
    }
    public async Task JoinServiceGroup(string serviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, serviceId);
    }
}
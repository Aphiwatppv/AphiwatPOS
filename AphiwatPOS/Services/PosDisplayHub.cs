using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AphiwatPOS.Services;

[AllowAnonymous]
public sealed class PosDisplayHub : Hub
{
    public Task JoinDisplay(string terminalId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(terminalId));
    }

    public Task SendDisplayUpdate(string terminalId, object payload)
    {
        return Clients.Group(GroupName(terminalId)).SendAsync("DisplayUpdated", payload);
    }

    private static string GroupName(string? terminalId)
    {
        var clean = string.IsNullOrWhiteSpace(terminalId) ? "default" : terminalId.Trim();
        return $"pos-display:{clean}";
    }
}

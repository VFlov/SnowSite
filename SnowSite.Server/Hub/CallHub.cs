using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SnowSite.Server.Hubs;
public class CallHub : Hub
{
    private static ConcurrentDictionary<string, string> RoomParticipants = new ConcurrentDictionary<string, string>();
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Rooms = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

    public void CreateRoom(string roomName, string userId)
    {
        Rooms[roomName] = new ConcurrentDictionary<string, string>();
        Rooms[roomName][Context.ConnectionId] = userId;
        RoomParticipants[Context.ConnectionId] = roomName;
    }
    public void RemoveRoom(string roomName)
    {
        Rooms.TryRemove(roomName, out _);
    }
    public async Task GetRoomList()
    {
        var roomNames = Rooms.Keys.ToList();

        await Clients.Caller.SendAsync("ReceiveRoomList", roomNames);
    }

    public async Task JoinRoom(string roomName, string userId)
    {

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        if (!Rooms.ContainsKey(roomName))
        {
            CreateRoom(roomName, userId);
        }
        await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId, userId);


    }


    public async Task LeaveRoom()
    {
        if (RoomParticipants.ContainsKey(Context.ConnectionId))
        {
            var roomName = RoomParticipants[Context.ConnectionId];
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

            if (Rooms.ContainsKey(roomName))
            {
                Rooms[roomName].TryRemove(Context.ConnectionId, out _);
                if (Rooms[roomName].Count == 0)
                {
                    RemoveRoom(roomName);
                }
            }

            RoomParticipants.TryRemove(Context.ConnectionId, out _);
            await Clients.Group(roomName).SendAsync("UserLeft", Context.ConnectionId);
        }
    }


    public async Task SendSignal(string signal, string targetConnectionId)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveRoom();
        await base.OnDisconnectedAsync(exception);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnowSite.Server.Hubs
{
    public class CallHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> RoomParticipants = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Rooms = new();
        private static readonly ConcurrentDictionary<string, List<(string UserId, string Message)>> RoomChatHistory = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await base.OnConnectedAsync();
        }

        public async Task CreateRoom(string roomName, string userId)
        {
            Console.WriteLine($"CreateRoom: roomName={roomName}, userId={userId}");
            Rooms[roomName] = new ConcurrentDictionary<string, string>();
            Rooms[roomName][Context.ConnectionId] = userId;
            RoomParticipants[Context.ConnectionId] = roomName;
            RoomChatHistory[roomName] = new List<(string, string)>();
            await GetRoomListForAll();
        }

        private async Task GetRoomListForAll()
        {
            var roomList = Rooms
                .Where(r => !string.IsNullOrEmpty(r.Key))
                .Select(r => new { Name = r.Key, ParticipantCount = r.Value.Count })
                .ToList();
            await Clients.All.SendAsync("ReceiveRoomList", roomList);
        }

        public void RemoveRoom(string roomName)
        {
            Console.WriteLine($"RemoveRoom: roomName={roomName}");
            Rooms.TryRemove(roomName, out _);
            RoomChatHistory.TryRemove(roomName, out _);
        }

        public async Task GetRoomList()
        {
            var roomList = Rooms.Select(r => new { Name = r.Key, ParticipantCount = r.Value.Count }).ToList();
            await Clients.Caller.SendAsync("ReceiveRoomList", roomList);
        }

        public async Task JoinRoom(string roomName, string userId)
        {
            Console.WriteLine($"JoinRoom: roomName={roomName}, userId={userId}, ConnectionId={Context.ConnectionId}");
            if (string.IsNullOrEmpty(roomName)) roomName = "UnnamedRoom";
            if (string.IsNullOrEmpty(userId)) userId = Context.ConnectionId;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            if (!Rooms.ContainsKey(roomName))
            {
                await CreateRoom(roomName, userId);
            }
            else
            {
                Rooms[roomName][Context.ConnectionId] = userId;
                RoomParticipants[Context.ConnectionId] = roomName;
            }


            if (RoomChatHistory.TryGetValue(roomName, out var history))
            {
                var historyToSend = history.Select(h => new { UserId = h.UserId, Message = h.Message }).ToList();
                Console.WriteLine($"Sending chat history to {Context.ConnectionId}: {history.Count} messages");
                foreach (var msg in historyToSend)
                {
                    Console.WriteLine($"History item - UserId: {msg.UserId}, Message: {msg.Message}");
                }
                await Clients.Caller.SendAsync("ReceiveChatHistory", historyToSend);
            }
            else
            {
                Console.WriteLine($"No chat history found for room {roomName}");
            }

            await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId, userId);
            await Clients.Caller.SendAsync("JoinedRoom", roomName);
            await GetRoomListForAll();
        }
        public async Task LeaveRoom()
        {
            if (RoomParticipants.TryGetValue(Context.ConnectionId, out var roomName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

                if (Rooms.TryGetValue(roomName, out var participants))
                {
                    participants.TryRemove(Context.ConnectionId, out _);
                    if (participants.Count == 0)
                    {
                        RemoveRoom(roomName);
                    }
                }

                RoomParticipants.TryRemove(Context.ConnectionId, out _);
                await Clients.Group(roomName).SendAsync("UserLeft", Context.ConnectionId);
                await Clients.Caller.SendAsync("LeftRoom", roomName);
                await GetRoomListForAll();
            }
        }

        public async Task SendChatMessage(string roomName, string userId, string message)
        {
            Console.WriteLine($"SendChatMessage: roomName={roomName}, userId={userId}, message={message}");
            if (Rooms.ContainsKey(roomName))
            {
                if (!RoomChatHistory.ContainsKey(roomName))
                {
                    RoomChatHistory[roomName] = new List<(string, string)>();
                }
                RoomChatHistory[roomName].Add((userId, message));
                Console.WriteLine($"Stored message in history - UserId: {userId}, Message: {message}");

                if (RoomChatHistory[roomName].Count > 100)
                {
                    RoomChatHistory[roomName].RemoveAt(0);
                }

                await Clients.Group(roomName).SendAsync("ReceiveChatMessage", userId, message);
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
}
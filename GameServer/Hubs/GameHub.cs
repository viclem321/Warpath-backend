using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using GameServer.Datas;
using GameServer.Services;
using GameServer.Models;


namespace GameServer.Hubs;

public class GameHub : Hub
{
    private readonly IMongoCollection<User> _users;
    private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
    public GameHub(MongoDBContext dbContext) {
        _users = dbContext.GetCollection<User>("Users");
    }
    public override async Task OnConnectedAsync()
    {
        User? user = await GetIdentity(Context.User ?? new ClaimsPrincipal()); if (user != null)
        {
            if (_connections.ContainsKey(user.username)) {  _connections.Remove(user.username); }
            _connections[user.username] = Context.ConnectionId;
        }
        await base.OnConnectedAsync();
    }
    public async Task<User?> GetIdentity(ClaimsPrincipal claimUser)
    {
        try {
            if (claimUser?.Identity != null && claimUser.Identity.IsAuthenticated) {
                string? username = claimUser.FindFirst("username")?.Value;
                User user = await _users.Find(u => u.username == username).FirstOrDefaultAsync(); if (user == null) { return null; }
                return user;
            } else { return null; }
        } catch { return null; }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        User? user = await GetIdentity(Context.User ?? new ClaimsPrincipal());
        if (user != null && _connections.ContainsKey(user.username))
        {
            _connections.Remove(user.username);
        }

        await base.OnDisconnectedAsync(exception);
    }






    // Envoyer un message à tous les clients connectés
    public async Task SendMessageToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    // Envoyer un message à un client 
    public async Task SendMessageToOne(string username, string nameMethod, string message)
    {
        if (_connections.ContainsKey(username))
        {
            var connectionId = _connections[username];
            await Clients.Client(connectionId).SendAsync(nameMethod, message);
        }
    }
}
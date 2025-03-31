using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Warpath.Shared.DTOs;
using GameServer.Datas;
using GameServer.Models;
using MongoDB.Bson;

namespace GameServer.Services;




public class L1UserServices {

    private readonly L2PlayerServices _playerServices;
    private readonly IMongoCollection<User> _users;  private readonly IMongoCollection<Player> _players;
    private readonly IConfiguration _configuration; private string? issuer; private string? audience; private string? key;

    public L1UserServices(MongoDBContext dbContext, IConfiguration configuration, L2PlayerServices playerServices)
    {
        _playerServices = playerServices;
        _users = dbContext.GetCollection<User>("Users");  _players = dbContext.GetCollection<Player>("Players");
        _configuration = configuration;
        issuer = _configuration["Jwt:Issuer"]; if(issuer == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
        audience = _configuration["Jwt:Audience"]; if(audience == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
        key = _configuration["Jwt:Key"]; if(key == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
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
    public async Task<User?> GetIdentityWithLock(ClaimsPrincipal claimUser)
    {
        if (claimUser?.Identity != null && claimUser.Identity.IsAuthenticated) {
            string? username = claimUser.FindFirst("username")?.Value;
            
            int i = 0;
            while(i < 4) {
                int currentTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                int newLockUntil = currentTimestamp + 20;

                var filter = Builders<User>.Filter.And( Builders<User>.Filter.Eq( "username", username ), Builders<User>.Filter.Lt("lockUntil", currentTimestamp) );
                var update = Builders<User>.Update.Set("lockUntil", newLockUntil); // Met à jour le lock
                var options = new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After };

                try { User user = await _users.FindOneAndUpdateAsync(filter, update, options); if(user != null) { return user; } } catch { return null; }
                Thread.Sleep(100); i++;
            }
            return null;  // and add in logs
        } else { return null; }
    }

    public async Task<bool> ReleaseLock(User user)
    {
        try {
            var update = Builders<User>.Update.Set("lockUntil", 0);
            await _users.UpdateOneAsync(Builders<User>.Filter.Eq("username", user.username), update);
            return true;
        } catch { Console.WriteLine("Impossible de unlock un user."); return false; }
    }




    // CONTROLLER CALL THESE -------------
    
    public async Task<bool> RegisterAsync(string? username, string? password)
    {
        try {
            if(username == null || password == null) { return false; }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            User user = new User(username, passwordHash, "");
            await _users.InsertOneAsync(user);
            return true;
        } catch { return false; }
    }

    public async Task<string?> LoginAsync(string? username, string? password)
    {
        try {
            // Recherche de l'utilisateur par username
            User user = await _users.Find(u => u.username == username).FirstOrDefaultAsync(); if (user == null) { return null; }
            // Vérification du mot de passe
            if (!BCrypt.Net.BCrypt.Verify(password, user.passwordHash)) { return null; }
            // Générer un token JWT
            string? token = GenerateJwtToken(user);
            return token;
        } catch { return null;}
    }
    private string? GenerateJwtToken(User user)
    {
        try {
            if(key == null) {return null;}
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("username", user.username ?? ""),
                new Claim("player", user.player ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken( issuer: issuer, audience: audience, claims: claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        } catch { return null; }
    }




    public async Task<PlayerDto?> CreatePlayer(User user, string newPseudo)
    {
        // verify if there isnt already a player for this user
        if(user.player != "") { return null; }
        // create new player and insert inside player collection
        Player newPlayer = new Player(newPseudo, new List<int>() );
        try { await _players.InsertOneAsync(newPlayer); } catch { return null; }
        // and new player inside user field
        try { await _users.UpdateOneAsync( Builders<User>.Filter.Eq(u => u.username, user.username), Builders<User>.Update.Set(u => u.player, newPlayer.pseudo) ); }  catch { Console.WriteLine("Important bug in CreatePlayer!"); return null; }
        // return new player to the controller
        return newPlayer.ToDto();
    }

    public string GetPlayerName(User user)
    {
        return user.player;
    }









}

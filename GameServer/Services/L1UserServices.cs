using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using GameServer.Datas;
using GameServer.Models;
using MongoDB.Bson;
using GameServer.DTOs;

namespace GameServer.Services;




public class L1UserServices {


    private readonly IMongoCollection<User> _users;  private readonly IMongoCollection<Player> _players;
    private readonly IConfiguration _configuration; private string? issuer; private string? audience; private string? key;

    public L1UserServices(MongoDBContext dbContext, IConfiguration configuration)
    {
        _users = dbContext.GetCollection<User>("Users");  _players = dbContext.GetCollection<Player>("Players");
        _configuration = configuration;
        issuer = _configuration["Jwt:Issuer"]; if(issuer == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
        audience = _configuration["Jwt:Audience"]; if(audience == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
        key = _configuration["Jwt:Key"]; if(key == null) { Console.WriteLine("Probleme config AuthService"); Environment.Exit(1); }
    }




    public async Task<bool> RegisterAsync(string? username, string? password)
    {
        try {
            if(username == null || password == null) { return false; }
            var existingUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync(); if (existingUser != null) { return false; }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, PasswordHash = passwordHash };
            await _users.InsertOneAsync(user);
            return true;
        } catch { return false; }
    }

    public async Task<string?> LoginAsync(string? username, string? password)
    {
        try {
            // Recherche de l'utilisateur par username
            User user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync(); if (user == null) { return null; }
            // Vérification du mot de passe
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) { return null; }
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
                new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? ""),
                new Claim("userId", user.Id.ToString() ?? ""),
                new Claim("playerId", user.playerId.ToString() ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken( issuer: issuer, audience: audience, claims: claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        } catch { return null; }
    }

    public async Task<User?> GetIdentity(ClaimsPrincipal claimUser)
    {
        try {
            if (claimUser?.Identity != null && claimUser.Identity.IsAuthenticated) {
                string? userId = claimUser.FindFirst("userId")?.Value;
                User user = await _users.Find(u => u.Id == ObjectId.Parse(userId)).FirstOrDefaultAsync(); if (user == null) { return null; }
                return user;
            } else { return null;}
        } catch { return null;}
    }



    public async Task<PlayerDto?> CreatePlayer(ClaimsPrincipal claimUser, string newPseudo)
    {
        try {
            User? user = await GetIdentity(claimUser); if( user == null) { return null; }
            if(user.playerId != null) { return null; }

            Player player = await _players.Find(p => p.pseudo == newPseudo).FirstOrDefaultAsync(); if(player != null) { return null; }
            Player newPlayer = new Player(newPseudo, null);
            await _players.InsertOneAsync(newPlayer);
            await _users.UpdateOneAsync( Builders<User>.Filter.Eq(u => u.Id, user.Id), Builders<User>.Update.Set(u => u.playerId, newPlayer._id) );
            return newPlayer.ToDto();
        } catch { return null; }
    } 


}

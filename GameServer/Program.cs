using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using GameServer.Datas;
using GameServer.Services;


var builder = WebApplication.CreateBuilder(args);
//BDD
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBContext>();  // Ajoute MongoDBContext
//Authentification
string? issuer = builder.Configuration["Jwt:Issuer"]; string? audience = builder.Configuration["Jwt:Audience"]; string? key = builder.Configuration["Jwt:Key"];
if(issuer == null || audience == null || key == null) {Console.WriteLine("Impossible de configurer l'authentification."); Environment.Exit(1);}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = issuer, ValidAudience = audience, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});
//Services
builder.Services.AddSingleton<L1UserServices>();
builder.Services.AddSingleton<L2PlayerServices>();
builder.Services.AddSingleton<L3MapServices>();
builder.Services.AddSingleton<L4VillageServices>();
builder.Services.AddSingleton<L5BuildingServices>();
//Controllers
builder.Services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.TypeNameHandling = TypeNameHandling.None; });
//build
var app = builder.Build();
//instanciation des services Singleton
var userService = app.Services.GetRequiredService<L1UserServices>();var playerService = app.Services.GetRequiredService<L2PlayerServices>(); var mapService = app.Services.GetRequiredService<L3MapServices>();  var villageService = app.Services.GetRequiredService<L4VillageServices>(); var buildingService = app.Services.GetRequiredService<L5BuildingServices>();



//Middlewares

app.UseAuthentication();

app.MapControllers();

app.Run();

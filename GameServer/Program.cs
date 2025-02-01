using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GameServer.Datas;
using GameServer.Services;


var builder = WebApplication.CreateBuilder(args);
//BDD
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBContext>();  // Ajoute MongoDBContext
//Services
builder.Services.AddSingleton<VillageServices>();
//Controllers
builder.Services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.TypeNameHandling = TypeNameHandling.None; });
//build
var app = builder.Build();




app.UseExceptionHandler("/error");

app.MapControllers();

app.Run();

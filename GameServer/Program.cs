using Microsoft.EntityFrameworkCore;
using GameServer.Services;
using GameServer.datas;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<GlobalServices>();
builder.Services.AddControllers();
var app = builder.Build();




app.UseExceptionHandler("/error");

app.MapControllers();

app.Run();

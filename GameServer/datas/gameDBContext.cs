using System.Data;
using GameServer.Models;
using Microsoft.EntityFrameworkCore;


namespace GameServer.datas {

    public class GameDbContext : DbContext {

        public DbSet<Village> Villages { get; set; }


        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options){ }
    }

}
using Microsoft.EntityFrameworkCore;
using GameServer.Models;
using GameServer.datas;


namespace GameServer.Services {

    
    public class GlobalServices {
        private readonly GameDbContext _context;

        public GlobalServices(GameDbContext context) {
            _context = context;
        }

        public async Task<List<Village>> GetAllVillagesAsync() {
            return await _context.Villages.ToListAsync();
        }

        public async Task CreateVillageAsync(Village village) {
            _context.Villages.Add(village);
            await _context.SaveChangesAsync();
        }
    }

}
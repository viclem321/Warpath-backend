using GameServer.Models;

namespace GameServer.DTOs;



public class VillageDto
{
    public string? id { get; set; }
    public string? owner {get; set; }
    public int positionX {get; set; }   public int positionY { get; set; }
    //BUILDINGS
    public List<BuildingDto>? buildings { get; set; }

}




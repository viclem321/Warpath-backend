using GameServer.Models;

namespace GameServer.DTOs;



public class VillageDto
{
    public List<BuildingDto> buildings { get; set; }
    public UpgradeAction upgradeAction1 { get; set; }

    public VillageDto(List<BuildingDto> pBuildings, UpgradeAction pUpgradeAction1) {
        buildings = pBuildings; upgradeAction1 = pUpgradeAction1;
    }

}




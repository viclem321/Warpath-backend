
namespace GameServer.DTOs;


public class BuildingDto { public string? buildingType; public int level = 0; public bool isInConstruction = false; }


public class HqDTO : BuildingDto { }



public class ResourceBuildingDto : BuildingDto {
    public int quantity { get; set;}
    public DateTime lastHarvest { get; set; }
}
public class ScierieDTO : ResourceBuildingDto {
}
public class FermeDTO : ResourceBuildingDto {
}
public class MineDTO : ResourceBuildingDto {
}

public class EntrepotDTO : BuildingDto {
    public List<int> stock { get; set; } = new();
}

public class CampMilitaireDTO : BuildingDto {
    public int nSoldats; public int nSoldatsDisponible;
}

public class CaserneDTO : BuildingDto {
    public bool isTraining; public DateTime endTrainingAt; public int nSoldatsTraining;
}





public enum ResourceType { Wood = 0, Food = 1, Oil = 2 }
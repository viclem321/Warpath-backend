
namespace GameServer.DTOs;


public class BuildingDto { public string? buildingType; public int level = 0; }


public class HqDTO : BuildingDto { }

public class ScierieDTO : BuildingDto {
    public int quantity { get; set;}
    public DateTime lastHarvest { get; set; }
}

public class FermeDTO : BuildingDto {
    public int quantity { get; set; }
    public DateTime lastHarvest { get; set; }
}

public class MineDTO : BuildingDto {
    public int quantity { get; set; }
    public DateTime lastHarvest { get; set; }
}

public class EntrepotDTO : BuildingDto {
    public int woodQuantity { get; set; }  public int foodQuantity { get; set; }  public int oilQuantity { get; set; }
}



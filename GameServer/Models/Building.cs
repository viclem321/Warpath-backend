using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using GameServer.Catalogue;
using GameServer.DTOs;

namespace GameServer.Models;






public abstract class Building {
    public string buildingType { get; set; }
    public int level { get; set; }

    public Building() { buildingType = this.GetType().Name; level = 0; }
    public bool Upgrade() {
        try {
            JObject building = CatalogueGlobal.buildings[this.GetType().Name];
            if( level + 1 <= ((int?)building["MaxLevel"] ?? 0) ) { level += 1; return true; }
            else { return false;}
        } catch { return false;}
    }
    public abstract BuildingDto ToDto();
}



public class Hq : Building {

    public Hq() { }
    public override BuildingDto ToDto()
    {
        return new HqDTO { buildingType = this.buildingType,  level = this.level, };
    }
}

public class Scierie : Building {
    public int quantity { get; set; }
    public DateTime lastHarvest  { get; set; }

    public Scierie() { quantity = 0; lastHarvest = DateTime.UtcNow; }

    public override BuildingDto ToDto()
    {
        return new ScierieDTO { buildingType = this.buildingType, level = this.level,  quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}

public class Ferme : Building {
    public int quantity { get; set; }
    public DateTime lastHarvest { get; set; }

    public Ferme() { quantity = 0; lastHarvest = DateTime.UtcNow; }

    public override BuildingDto ToDto()
    {
        return new FermeDTO { buildingType = this.buildingType, level = this.level, quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}

public class Mine : Building {
    public int quantity { get; set; }
    public DateTime lastHarvest { get; set; }

    public Mine() { quantity = 0; lastHarvest = DateTime.UtcNow; }

    public override BuildingDto ToDto()
    {
        return new MineDTO { buildingType = this.buildingType, level = this.level, quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}

public class Entrepot : Building {
    public int woodQuantity { get; set; } public int foodQuantity { get; set; } public int oilQuantity { get; set; }

    public Entrepot() { woodQuantity = 0; foodQuantity = 0; oilQuantity = 0; }

    public override BuildingDto ToDto()
    {
        return new EntrepotDTO { buildingType = this.buildingType, level = this.level, woodQuantity = this.woodQuantity, foodQuantity = this.foodQuantity, oilQuantity = this.oilQuantity, };
    }
}
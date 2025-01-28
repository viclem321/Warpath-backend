using System.Numerics;

namespace GameServer.Models
{
    public class Village {
        public int Id { get; set; }
        public string owner {get; set; }
        public int positionX {get; set; }
        public int positionY { get; set; }
        //buildings
        public int hqLevel { get; set; }
        public int buildingWoodLevel { get; set; }   public int woodQuantity { get; set; }   public int woodProduction { get; set; }   public int woodCapacity { get; set; }   public DateTime woodLastHarvest { get; set; }
        public int buildingFoodLevel { get; set; }   public int foodQuantity { get; set; }   public int foodProduction { get; set; }   public int foodCapacity { get; set; }   public DateTime foodLastHarvest { get; set; }
        public int buildingOilLevel { get; set; }    public int oilQuantity { get; set; }    public int oilProduction { get; set; }   public int oilCapacity { get; set; }   public DateTime oilLastHarvest { get; set; }
        public int academieLevel { get; set; }
        public int entrepotLevel { get; set; }


        public Village(){ }
        public Village(string pOwner, int pX, int pY){
            owner = pOwner;
            positionX = pX;   positionY = pY;
            //buildings
            hqLevel = 1;
            buildingWoodLevel = 0; woodQuantity = 0; woodProduction = 0;
            buildingFoodLevel = 0; foodQuantity = 0; foodProduction = 0;
            buildingOilLevel = 0; oilQuantity = 0;  oilProduction = 0;
            academieLevel = 0; entrepotLevel = 0;
        }
    }
}
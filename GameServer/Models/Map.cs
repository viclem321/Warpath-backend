using System.Numerics;
using Microsoft.AspNetCore.Components;

namespace GameServer.Models
{

    public class Map {
        public int width { get; private set; }
        public int height { get; private set; }
        public Tile[,] tiles { get; private set; }

        public Map(int pWidth, int pHeight) {
            width = pWidth;   height = pHeight;
            
            tiles = new Tile[width, height];
            for(int x = 0; x < width; x++ ) {
                for(int y = 0; y < height; y++) {
                    tiles[x,y] = new Tile(x, y, TileType.Empty);
                }
            }
        }

        public bool PlaceVillage(int pX, int pY, Village pVillage){
            if(!IsinBounds(pX, pY)) { return false; }
            if(tiles[pX, pY].PlaceVillage(pVillage) == false) { return false; }
            return true;
        }
        public bool RemoveVillage(int pX, int pY, Village pVillage){
            if(!IsinBounds(pX, pY)) { return false; }
            if(tiles[pX, pY].RemoveVillage(pVillage) == false) { return false; }
            return true;
        }

        public bool PlaceRessource(int pX, int pY, RessourceType pRessourceType){
            if(!IsinBounds(pX, pY)) { return false; }
            if(tiles[pX, pY].PlaceRessource(pRessourceType) == false) { return false; }
            return true;
        }
        public bool RemoveRessource(int pX, int pY){
            if(!IsinBounds(pX, pY)) { return false; }
            if(tiles[pX, pY].RemoveRessource() == false) { return false; }
            return true;
        }


        private bool IsinBounds(int pX, int pY){
            return pX >= 0 && pX < width && pY >= 0 && pY < height;
        }
    }






    public class Tile {
        public int x { get; set; }
        public int y { get; set; }
        public TileType type { get; set; }
        public Village? village { get; set; }
        public Ressource? ressource { get; set; }

        public Tile(int pX, int pY, TileType pType) {
            x = pX;  y = pY;
            type = pType;
        }


        public bool PlaceVillage(Village pVillage){
            if(type != TileType.Empty) { return false; }
            
            type = TileType.Village;
            village = pVillage;
            return true;
        }
        public bool RemoveVillage(Village pVillage){
            if(type != TileType.Village || village != pVillage) { return false; }
            
            type = TileType.Empty;
            village = null;
            return true;
        }

        public bool PlaceRessource(RessourceType pRessourceType){
            if(type != TileType.Empty) { return false; }
            
            type = TileType.Ressource;
            switch(pRessourceType) {
                case RessourceType.Food:
                    ressource = new Ressource(x, y, RessourceType.Food);
                    break;
                case RessourceType.Wood:
                    ressource = new Ressource(x, y, RessourceType.Wood);
                    break;
                case RessourceType.Oil:
                    ressource = new Ressource(x, y, RessourceType.Oil);
                    break;
            }
            return true;
        }
        public bool RemoveRessource(){
            if(type != TileType.Ressource) { return false; }
            
            type = TileType.Empty;
            ressource = null;
            return true;
        }
    }
    public enum TileType { Empty, Village, Ressource}








    public class Ressource {
        public int x { get; set; }
        public int y { get; set; }
        public RessourceType type { get; set; }
        public int quantity { get; set; }
        public int maxWood = 100;  public int maxFood = 100;   public int maxOil = 100;

        public Ressource(int pX, int pY, RessourceType pType) {
            x = pX;  y = pY;
            type = pType;
            switch(type){
                case RessourceType.Wood:
                    quantity = maxWood; break;
                case RessourceType.Food:
                    quantity = maxFood; break;
                case RessourceType.Oil:
                    quantity = maxOil; break;
            }
        }
    }
    public enum RessourceType { Wood, Food, Oil }
}
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameServer.Catalogue {

    public static class CatalogueGlobal {

        // BATIMENTS
        public static Dictionary<string, JObject> buildings {get; private set; } = new Dictionary<string, JObject>();


        static CatalogueGlobal() {
            try {
                string filePath = Path.Combine(AppContext.BaseDirectory, "Catalogue", "CatalogueGlobal.json");
                if(File.Exists(filePath)) {
                    string jsonText = File.ReadAllText(filePath);
                    JObject datas = JObject.Parse(jsonText);
                    JObject? buildingsDatas = datas["All"]?["Batiments"] as JObject; if(buildingsDatas == null) {Console.WriteLine("ERREUR, Fichier json incorrect !"); Environment.Exit(1);}
                    foreach (var building in buildingsDatas) {
                        if(building.Value is JObject buildingObject) { buildings[building.Key] = buildingObject; } 
                        else { Console.WriteLine("ERREUR, Fichier json incorrect !"); Environment.Exit(1); }
                    }
                } else { Console.WriteLine("ERREUR, Fichier json introuvable !"); Environment.Exit(1); }
            } catch (Exception ex) { Console.WriteLine($"ERREUR de chargement du fichier json ! {ex}"); Environment.Exit(1); }
        }
        
    }


}
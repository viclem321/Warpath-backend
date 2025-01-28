using System;
using System.IO;
using Newtonsoft.Json;

namespace GameServer.Catalogue {

    public static class CatalogueGlobal {
        public static dynamic? Data;
        static CatalogueGlobal() {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Catalogue", "CatalogueGlobal.json");
            if(File.Exists(filePath)) {
                string json = File.ReadAllText(filePath);
                Data = JsonConvert.DeserializeObject<dynamic>(json);
            } else {
                throw new FileNotFoundException($"Le fichier {filePath} est introuvable.");
            }
        }

        public static dynamic GetBatiment(string nom) => Data? ["Batiments"][nom];
        
    }


}
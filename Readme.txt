
SI COPIE DU PROJET SUR UN AUTRE ORDINTEUR WINDOWS : 

    -installer .NET version 9
    -installer mongodb pour Windows, créer une database nommée "GameServerDB" qui tourne en localhost sur le port 27017
    -installer Visual studio code (avec l'extension Web C# Devkit)
    
    -copier le projet (sans les dossiers bin et obj). Vous pouvez ouvrir le dossier avec Visual studio code.
    -exécuter "dotnet restore" dans le dossier du projet pour installer toutes les dépendances requises.
    -si besoin, vérifier la chaine de connexion à la base de donnée dans appsettings.json/appsettings.Development.json
    -lancer le programme avec dotnet run



VARIABLES D'ENVIRONNEMENTS A CONFIGURER SI BESOIN:

    - ASPNETCORE_ENVIRONMENT=[Development/Production]   (peu être directement configuré dans Properties/launchSettings)





LES ENDPOINTS:

    S'inscrire : POST  http://localhost:5245/api/register   (  body :  { "Username": "user", "Password": "pass" }  )
    Se login (récuparation du token) : POST  http://localhost:5245/api/login   (  body :  { "Username": "user", "Password": "pass" }  )
    Create a player for a user (need token) : POST http://localhost:5245/api/user/{username}/createPlayer  (body : "pseudo")
    Get PlayerName of a user (need token): GET http://localhost:5245/api/user/{username}/getPlayerName
    Get all Datas about a player (need token + playerName) : GET http://localhost:5245/api/user/{username}/player/{playerName}/getDatas
    Create a new village for a player (need token + playerName + newVillage location) : POST http://localhost:5245/api/user/{username}/player/{playerName}/createNewVillage   ( body : { indexNewVillage }  )
    Delete a village  (need token + playerName + village location) : POST http://localhost:5245/api/user/{username}/player/{playerName}/deleteVillage   ( body : { indexVillage } )

    Get basics informations about a mapTile if player have it in his vision (need token + playerName + location of the tile):  GET http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/getOneTile
    Special endpoint for getting all the map:   GET http://localhost:5245/api/user/{username}/player/{playerName}/map/getAllMap
    Special endpoint for getting all villages:   GET http://localhost:5245/api/user/{username}/player/{playerName}/map/{index}/village/getAllVillages
    Get all datas about a village if owned by the player (need token + playerName + mapLocation): GET http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/village/getVillageDatas
    
    Start Upgrade a building inside a village if you are the owner of the village and if you have rss (need token + playerName + mapLocation + buildingType):  POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/village/building/{buildingType}/startUpgradeBuilding   ( body: )
    End Upgrade if time is finish (need token + playerName + villageLocation  ):  POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/village/endUpgradeAction1   ( body: )

    Send soldiers training in caserne (need token + playerName + mapLocation + nSoldiers):   POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/village/building/caserne/trainingTroops/{nSoldats}
    End training inside Caserne and send troops in CampMilitaire (need token + playerName + mapLocation):   POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/village/building/caserne/endTraining

    Attack a other village (need token + playerName + mapLocation + defenseurLocation + nSoldats):   POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{indexTile}/attack/{indexTileToAttack}  ( body : { nSoldats }  )


NOTES PERSO : 

-Pour le moment, garder la Map en BDD dans un seul document, et possiblement à l'avenir séparer la Map en chunks, ou/et garder une copie de la Map en mémoire vive pour les accès en lecture.


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
    Create a new village for a player (need token + playerName + newVillage location) : POST http://localhost:5245/api/user/{username}/player/{playerName}/createNewVillage   (body : { "locationX": {locX}, "locationY": {locY} }  )
    Get basics informations about a mapTile if player have it in his vision (need token + playerName + location of the tile):  GET http://localhost:5245/api/user/{username}/player/{playerName}/map/getOneTile/{locX}&{locY}
    Special endpoint for getting all the map:   GET http://localhost:5245/api/user/{username}/player/{playerName}/map/getAllMap
    Special endpoint for getting all villages:   GET http://localhost:5245/api/user/{username}/player/{playerName}/map/{locX}&{locY}/village/getAllVillages
    Get all datas about a village if owned by the player (need token + playerName + mapLocation): GET http://localhost:5245/api/user/{username}/player/{playerName}/map/{locX}&{locY}/village/getAllDatas
    Upgrade a building inside a village if you are the owner of the village (need token + playerName + mapLocation + buildingType):  POST http://localhost:5245/api/user/{username}/player/{playerName}/map/{locX}&{locY}/village/upgradeBuilding   ( body: {buildingType} )






NOTES PERSO : 

-Possiblement à l'avenir organiser la Map en chunks en BDD, ou/et garder une copie de la Map en mémoire vive pour les accès en lecture.

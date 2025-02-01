
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

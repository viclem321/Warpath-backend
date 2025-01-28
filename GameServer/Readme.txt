
SI COPIE DU PROJET SUR UN AUTRE ORDINTEUR WINDOWS : 

    -installer .NET version 9
    -utiliser la commande "dotnet tool install --global dotnet-ef"
    -installer sql server, avec Authentification WIndows La base de donnée doit s'appeler "master"
    -si besoin, installer Sql Server Management Studio (SSMS) pour authoriser l'user du gameServer d'accéder à la base de donnée
    -installer Visual studio code (avec l'extension Web C# Devkit)
    
    -copier le projet (sans les dossiers bin et obj). Vous pouvez ouvrir le dossier avec Visual studio code.
    -exécuter "dotnet restore" dans le dossier du projet pour installer toutes les dépendances requises.
    -si besoin, vérifier la chaine de connexion SQL server dans appsettings.json
    -pour initialiser la BDD utilise les commandes "dotnet ef migrations remove"(si necessaire pour supprimer la derniere migration) , "dotnet ef migrations add InitialCreate"(pour créer un script d'initialisation de DBB qui contient toues les bonnes structure que tu utilise dans ton projet) et "dotnet ef database update" pour appliquer la migration en question sur la DBB.
    -lancer le programme avec dotnet run





VARIABLES D'ENVIRONNEMENTS A CONFIGURER SI BESOIN:

    - ASPNETCORE_ENVIRONMENT=[Development/Production]   (peu être directement configuré dans Properties/launchSettings)

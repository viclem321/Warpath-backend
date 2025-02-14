using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GameServer.Models;




public class MapTile
{
    [BsonId]
    public int _id;
    public int lockUntil;
    public TileType type; public ObjectId? dataId; 

    public MapTile(int pId, TileType pType, ObjectId? pDataId)
    {
        _id = pId; lockUntil = 0; 
        type = pType;   dataId = pDataId;
    }
}







public enum TileType { Empty = 0, Village = 1, Resource = 2, }
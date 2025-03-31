using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;

namespace GameServer.Models;




public class Map
{
    public List<MapTile> mapTiles;

    public Map(List<MapTile>? pMapTiles) {
        mapTiles = pMapTiles ?? new List<MapTile>();
    }

    public MapDto ToDto(List<string> pAllPseudo) {
        MapDto newMapDto = new MapDto();
        int counter1 = 0; foreach (MapTile mapTile in mapTiles) { newMapDto.mapTiles.Add(mapTile.ToDto(pAllPseudo[counter1])); counter1 ++; }
        return newMapDto;
    }
}


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

    public MapTileDTO ToDto(string pPseudo) {
        if(type == TileType.Village) { return new MapTileDTO { _id = this._id, type = this.type, owner = pPseudo }; }
        else { return new MapTileDTO { _id = this._id, type = this.type }; }
    }
}



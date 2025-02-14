using GameServer.Models;

namespace GameServer.DTOs;



public class PlayerDto
{
    public string? pseudo { get; set; }
    public List<int>? allMapVillages { get; set; }

}
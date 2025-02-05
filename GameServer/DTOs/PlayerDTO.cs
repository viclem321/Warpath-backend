using GameServer.Models;

namespace GameServer.DTOs;



public class PlayerDto
{
    public string? id { get; set; }
    public string? pseudo { get; set; }
    public List<string>? allVillages { get; set; }

}
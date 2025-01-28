using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    positionX = table.Column<int>(type: "int", nullable: false),
                    positionY = table.Column<int>(type: "int", nullable: false),
                    hqLevel = table.Column<int>(type: "int", nullable: false),
                    buildingWoodLevel = table.Column<int>(type: "int", nullable: false),
                    woodQuantity = table.Column<int>(type: "int", nullable: false),
                    woodProduction = table.Column<int>(type: "int", nullable: false),
                    woodCapacity = table.Column<int>(type: "int", nullable: false),
                    woodLastHarvest = table.Column<DateTime>(type: "datetime2", nullable: false),
                    buildingFoodLevel = table.Column<int>(type: "int", nullable: false),
                    foodQuantity = table.Column<int>(type: "int", nullable: false),
                    foodProduction = table.Column<int>(type: "int", nullable: false),
                    foodCapacity = table.Column<int>(type: "int", nullable: false),
                    foodLastHarvest = table.Column<DateTime>(type: "datetime2", nullable: false),
                    buildingOilLevel = table.Column<int>(type: "int", nullable: false),
                    oilQuantity = table.Column<int>(type: "int", nullable: false),
                    oilProduction = table.Column<int>(type: "int", nullable: false),
                    oilCapacity = table.Column<int>(type: "int", nullable: false),
                    oilLastHarvest = table.Column<DateTime>(type: "datetime2", nullable: false),
                    academieLevel = table.Column<int>(type: "int", nullable: false),
                    entrepotLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Villages");
        }
    }
}

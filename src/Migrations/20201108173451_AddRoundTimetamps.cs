using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sketch.Migrations
{
    public partial class AddRoundTimetamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDrawing",
                table: "PlayersTurns");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTimestamp",
                table: "Rounds",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimestamp",
                table: "Rounds",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimestamp",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "StartTimestamp",
                table: "Rounds");

            migrationBuilder.AddColumn<bool>(
                name: "IsDrawing",
                table: "PlayersTurns",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

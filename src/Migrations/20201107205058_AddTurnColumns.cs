using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sketch.Migrations
{
    public partial class AddTurnColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTimestamp",
                table: "Turns",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimestamp",
                table: "Turns",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimestamp",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "StartTimestamp",
                table: "Turns");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sketch.Migrations
{
    public partial class RemoveIgnore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Rounds_RoundId",
                table: "Turns");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoundId",
                table: "Turns",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsDrawing",
                table: "PlayersTurns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Turns_DrawingPlayerId",
                table: "Turns",
                column: "DrawingPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Players_DrawingPlayerId",
                table: "Turns",
                column: "DrawingPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Rounds_RoundId",
                table: "Turns",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Players_DrawingPlayerId",
                table: "Turns");

            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Rounds_RoundId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_Turns_DrawingPlayerId",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "IsDrawing",
                table: "PlayersTurns");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoundId",
                table: "Turns",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Rounds_RoundId",
                table: "Turns",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sketch.Migrations
{
    public partial class FixTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "GameRoomId",
                table: "Players",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameRoomId",
                table: "Players",
                column: "GameRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GameRoomId",
                table: "Players");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameRoomId",
                table: "Players",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}

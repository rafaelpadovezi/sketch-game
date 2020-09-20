using Microsoft.EntityFrameworkCore.Migrations;

namespace Sketch.Migrations
{
    public partial class UpdateRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameRoomId",
                table: "Rounds",
                column: "GameRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayersTurns_PlayerId",
                table: "PlayersTurns",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersTurns_Players_PlayerId",
                table: "PlayersTurns",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_GameRooms_GameRoomId",
                table: "Rounds",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayersTurns_Players_PlayerId",
                table: "PlayersTurns");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_GameRooms_GameRoomId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_GameRoomId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_PlayersTurns_PlayerId",
                table: "PlayersTurns");
        }
    }
}

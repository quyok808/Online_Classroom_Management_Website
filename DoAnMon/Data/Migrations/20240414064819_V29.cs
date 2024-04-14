using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Data.Migrations
{
    /// <inheritdoc />
    public partial class V29 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "classRooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_classRooms_UserId",
                table: "classRooms",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_classRooms_AspNetUsers_UserId",
                table: "classRooms",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classRooms_AspNetUsers_UserId",
                table: "classRooms");

            migrationBuilder.DropIndex(
                name: "IX_classRooms_UserId",
                table: "classRooms");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "classRooms");
        }
    }
}

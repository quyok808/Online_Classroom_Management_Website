using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostId",
                table: "classRooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassRoomId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classRooms_PostId",
                table: "classRooms",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_classRooms_posts_PostId",
                table: "classRooms",
                column: "PostId",
                principalTable: "posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classRooms_posts_PostId",
                table: "classRooms");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropIndex(
                name: "IX_classRooms_PostId",
                table: "classRooms");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "classRooms");
        }
    }
}

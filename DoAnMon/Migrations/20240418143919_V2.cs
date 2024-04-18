using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BaiNopIdBaiNop",
                table: "classRooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaiTapId",
                table: "classRooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassRoomId",
                table: "baiTaps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileFormat",
                table: "baiTaps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "baiTaps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attractUrl",
                table: "baiTaps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BaiNop",
                columns: table => new
                {
                    IdBaiNop = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaiTapId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Urlbainop = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiNop", x => x.IdBaiNop);
                    table.ForeignKey(
                        name: "FK_BaiNop_baiTaps_BaiTapId",
                        column: x => x.BaiTapId,
                        principalTable: "baiTaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classRooms_BaiNopIdBaiNop",
                table: "classRooms",
                column: "BaiNopIdBaiNop");

            migrationBuilder.CreateIndex(
                name: "IX_classRooms_BaiTapId",
                table: "classRooms",
                column: "BaiTapId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiNop_BaiTapId",
                table: "BaiNop",
                column: "BaiTapId");

            migrationBuilder.AddForeignKey(
                name: "FK_classRooms_BaiNop_BaiNopIdBaiNop",
                table: "classRooms",
                column: "BaiNopIdBaiNop",
                principalTable: "BaiNop",
                principalColumn: "IdBaiNop");

            migrationBuilder.AddForeignKey(
                name: "FK_classRooms_baiTaps_BaiTapId",
                table: "classRooms",
                column: "BaiTapId",
                principalTable: "baiTaps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classRooms_BaiNop_BaiNopIdBaiNop",
                table: "classRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_classRooms_baiTaps_BaiTapId",
                table: "classRooms");

            migrationBuilder.DropTable(
                name: "BaiNop");

            migrationBuilder.DropIndex(
                name: "IX_classRooms_BaiNopIdBaiNop",
                table: "classRooms");

            migrationBuilder.DropIndex(
                name: "IX_classRooms_BaiTapId",
                table: "classRooms");

            migrationBuilder.DropColumn(
                name: "BaiNopIdBaiNop",
                table: "classRooms");

            migrationBuilder.DropColumn(
                name: "BaiTapId",
                table: "classRooms");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "baiTaps");

            migrationBuilder.DropColumn(
                name: "FileFormat",
                table: "baiTaps");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "baiTaps");

            migrationBuilder.DropColumn(
                name: "attractUrl",
                table: "baiTaps");
        }
    }
}

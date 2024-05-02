using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class AddtableBangDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "baiTapsDetail");

            migrationBuilder.CreateTable(
                name: "bangDiem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassRoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DTB = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bangDiem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bangDiem_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bangDiem_classRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "classRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bangDiem_ClassRoomId",
                table: "bangDiem",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_bangDiem_UserId",
                table: "bangDiem",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bangDiem");

            migrationBuilder.CreateTable(
                name: "baiTapsDetail",
                columns: table => new
                {
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BaiTapId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassRoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_baiTapsDetail", x => new { x.ClassId, x.BaiTapId });
                    table.ForeignKey(
                        name: "FK_baiTapsDetail_baiTaps_BaiTapId",
                        column: x => x.BaiTapId,
                        principalTable: "baiTaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_baiTapsDetail_classRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "classRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_baiTapsDetail_BaiTapId",
                table: "baiTapsDetail",
                column: "BaiTapId");

            migrationBuilder.CreateIndex(
                name: "IX_baiTapsDetail_ClassRoomId",
                table: "baiTapsDetail",
                column: "ClassRoomId");
        }
    }
}

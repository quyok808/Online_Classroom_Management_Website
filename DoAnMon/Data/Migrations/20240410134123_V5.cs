using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Data.Migrations
{
    /// <inheritdoc />
    public partial class V5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "baiTaps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_baiTaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "classRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomOnline = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "baiTapsDetail",
                columns: table => new
                {
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BaiTapId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoomId = table.Column<string>(type: "nvarchar(450)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "classroomDetail",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoomId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classroomDetail", x => new { x.ClassId, x.UserId });
                    table.ForeignKey(
                        name: "FK_classroomDetail_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_classroomDetail_classRooms_ClassRoomId",
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

            migrationBuilder.CreateIndex(
                name: "IX_classroomDetail_ClassRoomId",
                table: "classroomDetail",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_classroomDetail_UserId",
                table: "classroomDetail",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "baiTapsDetail");

            migrationBuilder.DropTable(
                name: "classroomDetail");

            migrationBuilder.DropTable(
                name: "baiTaps");

            migrationBuilder.DropTable(
                name: "classRooms");
        }
    }
}

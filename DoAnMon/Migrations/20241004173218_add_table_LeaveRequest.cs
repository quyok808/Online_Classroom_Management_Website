using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class add_table_LeaveRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "leaveRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClassId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reasion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoomId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaveRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leaveRequest_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_leaveRequest_classRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "classRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_leaveRequest_ClassRoomId",
                table: "leaveRequest",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveRequest_UserID",
                table: "leaveRequest",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leaveRequest");
        }
    }
}

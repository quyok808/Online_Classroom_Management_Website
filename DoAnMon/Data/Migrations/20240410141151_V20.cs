using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Data.Migrations
{
    /// <inheritdoc />
    public partial class V20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                 name: "FK_classroomDetail_AspNetRoles_CustomRoleId",
                 table: "classroomDetail");

            migrationBuilder.DropIndex(
                name: "IX_classroomDetail_CustomRoleId",
                table: "classroomDetail");

            migrationBuilder.DropColumn(
                name: "CustomRoleId",
                table: "classroomDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classroomDetail_AspNetRoles_CustomRoleId",
                table: "classroomDetail");

            migrationBuilder.DropIndex(
                name: "IX_classroomDetail_CustomRoleId",
                table: "classroomDetail");

            migrationBuilder.DropColumn(
                name: "CustomRoleId",
                table: "classroomDetail");
        }
    }
}

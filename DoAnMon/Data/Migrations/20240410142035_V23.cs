using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Data.Migrations
{
    /// <inheritdoc />
    public partial class V23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomRoleId",
                table: "classroomDetail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_classroomDetail_CustomRoleId",
                table: "classroomDetail",
                column: "CustomRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_classroomDetail_AspNetRoles_CustomRoleId",
                table: "classroomDetail",
                column: "CustomRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
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

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");
        }
    }
}

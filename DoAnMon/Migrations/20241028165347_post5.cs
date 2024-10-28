using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class post5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leaveRequest_AspNetUsers_UserID",
                table: "leaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveRequest_classRooms_ClassRoomId",
                table: "leaveRequest");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "leaveRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClassRoomId",
                table: "leaveRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_leaveRequest_AspNetUsers_UserID",
                table: "leaveRequest",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_leaveRequest_classRooms_ClassRoomId",
                table: "leaveRequest",
                column: "ClassRoomId",
                principalTable: "classRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leaveRequest_AspNetUsers_UserID",
                table: "leaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveRequest_classRooms_ClassRoomId",
                table: "leaveRequest");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "leaveRequest",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ClassRoomId",
                table: "leaveRequest",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveRequest_AspNetUsers_UserID",
                table: "leaveRequest",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveRequest_classRooms_ClassRoomId",
                table: "leaveRequest",
                column: "ClassRoomId",
                principalTable: "classRooms",
                principalColumn: "Id");
        }
    }
}

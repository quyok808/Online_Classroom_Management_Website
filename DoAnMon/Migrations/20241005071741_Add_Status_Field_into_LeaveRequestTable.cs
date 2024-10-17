using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class Add_Status_Field_into_LeaveRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "leaveRequest");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "leaveRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "leaveRequest");

            migrationBuilder.AddColumn<string>(
                name: "ClassId",
                table: "leaveRequest",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

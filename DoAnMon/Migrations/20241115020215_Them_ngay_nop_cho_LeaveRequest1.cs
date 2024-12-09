using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class Them_ngay_nop_cho_LeaveRequest1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RubricId",
                table: "Students",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianYeuCau",
                table: "leaveRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThoiGianYeuCau",
                table: "leaveRequest");

            migrationBuilder.AlterColumn<int>(
                name: "RubricId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

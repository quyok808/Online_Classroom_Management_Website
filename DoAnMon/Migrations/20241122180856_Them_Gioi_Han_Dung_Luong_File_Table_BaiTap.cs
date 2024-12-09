using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
    public partial class Them_Gioi_Han_Dung_Luong_File_Table_BaiTap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSize",
                table: "baiTaps",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSize",
                table: "baiTaps");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMon.Migrations
{
    /// <inheritdoc />
<<<<<<<< HEAD:DoAnMon/Migrations/20240425090358_Add_Column_Diem.cs
    public partial class Add_Column_Diem : Migration
========
    public partial class Add_Check_Field_Into_Table_DiemDanh : Migration
>>>>>>>> abfb04d4b98687a60c442113bbf1b7367e516d05:DoAnMon/Migrations/20240912153006_Add_Check_Field_Into_Table_DiemDanh.cs
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
<<<<<<<< HEAD:DoAnMon/Migrations/20240425090358_Add_Column_Diem.cs
            migrationBuilder.AddColumn<decimal>(
                name: "Diem",
                table: "BaiNop",
                type: "decimal(18,2)",
========
            migrationBuilder.AddColumn<string>(
                name: "Check",
                table: "diemDanh",
                type: "nvarchar(max)",
>>>>>>>> abfb04d4b98687a60c442113bbf1b7367e516d05:DoAnMon/Migrations/20240912153006_Add_Check_Field_Into_Table_DiemDanh.cs
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
<<<<<<<< HEAD:DoAnMon/Migrations/20240425090358_Add_Column_Diem.cs
                name: "Diem",
                table: "BaiNop");
========
                name: "Check",
                table: "diemDanh");
>>>>>>>> abfb04d4b98687a60c442113bbf1b7367e516d05:DoAnMon/Migrations/20240912153006_Add_Check_Field_Into_Table_DiemDanh.cs
        }
    }
}

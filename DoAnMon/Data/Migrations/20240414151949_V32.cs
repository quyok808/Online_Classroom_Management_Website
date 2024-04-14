using Microsoft.EntityFrameworkCore.Migrations;

namespace DoAnMon.Data.Migrations
{
	public partial class V32 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Xóa khóa ngoại không cần thiết
			migrationBuilder.DropForeignKey(
				name: "FK_BaiGiang_classRooms_ClassId",
				table: "BaiGiang");

			// Xóa chỉ mục của cột ClassRoomId không cần thiết
			migrationBuilder.DropIndex(
				name: "IX_BaiGiang_ClassId",
				table: "BaiGiang");

			// Thay đổi kiểu dữ liệu của cột ClassId thành "nvarchar(450)"
			migrationBuilder.AlterColumn<string>(
				name: "ClassId",
				table: "BaiGiang",
				type: "nvarchar(450)",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)");

			// Tạo chỉ mục cho cột ClassId
			migrationBuilder.CreateIndex(
				name: "IX_baiGiang_ClassId",
				table: "BaiGiang",
				column: "ClassId");

			// Thêm khóa ngoại mới cho cột ClassId
			migrationBuilder.AddForeignKey(
				name: "FK_baiGiang_classRooms_ClassId",
				table: "BaiGiang",
				column: "ClassId",
				principalTable: "classRooms",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}

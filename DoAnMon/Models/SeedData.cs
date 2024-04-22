using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;

namespace DoAnMon.Models
{
	public static class SeedData
	{
		public static async Task Initialize(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			// Kiểm tra xem có người dùng nào tồn tại trong cơ sở dữ liệu chưa
			if (!userManager.Users.Any())
			{
				// Kiểm tra xem vai trò "Admin" đã tồn tại chưa
				var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
				if (!adminRoleExists)
				{
					// Nếu vai trò "Admin" chưa tồn tại, thêm nó vào cơ sở dữ liệu
					var adminRole = new IdentityRole("Admin");
					await roleManager.CreateAsync(adminRole);
				}

				// Tạo người dùng mặc định
				var user = new CustomUser
				{
					UserName = "1111111111",
					Email = "admin@onlya.com",
					// Các thông tin khác của người dùng
				};

				// Thêm người dùng vào cơ sở dữ liệu
				var result = await userManager.CreateAsync(user, "admin"); //password

				// Kiểm tra xem người dùng đã được tạo thành công hay không
				if (result.Succeeded)
				{
					// Thiết lập EmailConfirmed là true
					user.EmailConfirmed = true;
					// Cập nhật người dùng trong cơ sở dữ liệu
					await userManager.UpdateAsync(user);
					// Thêm người dùng vào vai trò "Admin"
					await userManager.AddToRoleAsync(user, "Admin");
				}
			}
		}

	}

}

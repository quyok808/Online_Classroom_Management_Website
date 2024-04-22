using DoAnMon.Areas.Admin.Models;
using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMon.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class HomeController : Controller
	{
		private readonly UserManager<CustomUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;


		public HomeController(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public IActionResult TrangChu()
		{
			return View();
		}

		public async Task<IActionResult> PhanQuyen()
		{
			var users = await _userManager.Users.ToListAsync();
			var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

			var userViewModels = new List<UserViewModel>();
			foreach (var user in users)
			{
				var userRoles = await _userManager.GetRolesAsync(user);
				var currentRole = userRoles.FirstOrDefault();

				var userViewModel = new UserViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					Name = user.Name, // Thay thế bằng thuộc tính tên đầy đủ của người dùng nếu có
					Email = user.Email,
					Roles = userRoles.Any() ? userRoles.ToList() : new List<string> { "No Role" }, // Thêm "No Role" nếu không có vai trò
					AllRoles = roles, // Thêm thuộc tính AllRoles vào UserViewModel
					CurrentRole = currentRole ?? "No Role" // Gán vai trò đầu tiên của người dùng hoặc "No Role" cho CurrentRole
				};


				userViewModels.Add(userViewModel);
			}

			return View(userViewModels);
		}

		[HttpPost]
		public async Task<IActionResult> UpdateUserRole(string userId, string roleName)
		{
			// Kiểm tra xem userId và roleName có giá trị hợp lệ hay không
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
			{
				return BadRequest("UserId và RoleName không được để trống.");
			}

			// Tìm kiếm người dùng
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("Không tìm thấy người dùng.");
			}

			// Kiểm tra xem vai trò đã tồn tại chưa
			var roleExists = await _roleManager.RoleExistsAsync(roleName);
			if (!roleExists)
			{
				return BadRequest("Vai trò không tồn tại.");
			}

			// Loại bỏ tất cả các vai trò hiện tại của người dùng
			var currentRoles = await _userManager.GetRolesAsync(user);
			var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
			if (!removeResult.Succeeded)
			{
				return BadRequest("Không thể loại bỏ các vai trò hiện tại của người dùng.");
			}

			// Thêm người dùng vào vai trò mới
			var addResult = await _userManager.AddToRoleAsync(user, roleName);
			if (!addResult.Succeeded)
			{
				return BadRequest("Không thể thêm người dùng vào vai trò mới.");
			}
			TempData["AlertMessage"] = "Vai trò đã được cập nhật thành công.";
			return Redirect("/Admin/Home/PhanQuyen");
		}
	}
}


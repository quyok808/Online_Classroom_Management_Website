using DoAnMon.Areas.Admin.Models;
using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMon.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // Trả về NotFound nếu không tìm thấy người dùng
            }

            // Kiểm tra xem vai trò đã tồn tại chưa
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                ModelState.AddModelError(string.Empty, "Vai trò không tồn tại.");
                return RedirectToAction("Index"); // Hoặc chuyển hướng đến trang khác tùy thuộc vào yêu cầu của ứng dụng
            }

            // Xác định vai trò hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);

            // Nếu người dùng đã có vai trò mới, không cần thực hiện bất kỳ thay đổi nào
            if (userRoles.Contains(roleName))
            {
                return RedirectToAction("Index");
            }

            // Xóa tất cả vai trò hiện tại của người dùng
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!removeRolesResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa vai trò hiện tại của người dùng.");
                return RedirectToAction("Index");
            }

            // Thêm vai trò mới cho người dùng
            var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!addRoleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Không thể thêm vai trò cho người dùng.");
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }




        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Người dùng không tồn tại
                return false;
            }

            // Kiểm tra xem vai trò có tồn tại không
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                // Nếu vai trò không tồn tại, tạo mới
                var role = new IdentityRole(roleName);
                var roleResult = await _roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    // Xử lý lỗi khi tạo vai trò không thành công
                    return false;
                }
            }

            // Thêm người dùng vào vai trò
            var result = await _userManager.AddToRoleAsync(user, roleName);

            return result.Succeeded;
        }

        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            // Tìm người dùng dựa trên userId
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // Trả về NotFound nếu không tìm thấy người dùng
            }

            // Kiểm tra xem vai trò đã tồn tại chưa
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                ModelState.AddModelError(string.Empty, "Vai trò không tồn tại.");
                return RedirectToAction("Index"); // Hoặc chuyển hướng đến trang khác tùy thuộc vào yêu cầu của ứng dụng
            }

            // Kiểm tra xem người dùng đã có vai trò đó chưa
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                ModelState.AddModelError(string.Empty, "Người dùng đã có vai trò này.");
                return RedirectToAction("Index"); // Hoặc chuyển hướng đến trang khác tùy thuộc vào yêu cầu của ứng dụng
            }

            // Thêm vai trò cho người dùng
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Không thể thêm vai trò cho người dùng.");
                return RedirectToAction("Index"); // Hoặc chuyển hướng đến trang khác tùy thuộc vào yêu cầu của ứng dụng
            }

            // Chuyển hướng người dùng đến trang Index sau khi thêm vai trò thành công
            return RedirectToAction("Index");
        }


    }
}


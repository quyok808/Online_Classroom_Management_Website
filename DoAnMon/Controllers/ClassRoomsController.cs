using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnMon.Data;
using DoAnMon.Models;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;
using static DoAnMon.Models.ClassroomViewModel;
using Microsoft.Extensions.Hosting;

namespace DoAnMon.Controllers
{
    public class ClassRoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ClassRoomsController(ApplicationDbContext context, UserManager<CustomUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: ClassRooms
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            List<ClassRoomViewModel> classRoomViewModels = new List<ClassRoomViewModel>();
            List<ClassRoom> userClasses = null;

            if (currentUser != null)
            {
                // Kiểm tra xem người dùng có trong bảng class hay không
                var isTeacher = await _context.classRooms.AnyAsync(p => p.UserId == currentUser.Id);

                if (isTeacher)
                {
                    // Lấy danh sách lớp học mà người dùng là chủ sở hữu từ bảng classRooms
                    userClasses = await _context.classRooms.Where(p => p.UserId == currentUser.Id).ToListAsync();
                }
                else
                {
                    // Lấy danh sách lớp học mà người dùng có ID trong bảng ClassroomDetail
                    var classDetailClasses = await _context.classroomDetail
                        .Where(p => p.UserId == currentUser.Id)
                        .Select(p => p.ClassId)
                        .ToListAsync();

                    userClasses = await _context.classRooms
                        .Where(p => classDetailClasses.Contains(p.Id))
                        .ToListAsync();
                }

                foreach (var classRoom in userClasses)
                {
                    var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == classRoom.UserId);

                    // Kiểm tra null cho owner trước khi thêm vào classRoomViewModels
                    if (owner != null)
                    {
                        classRoomViewModels.Add(new ClassRoomViewModel
                        {
                            ClassRoom = classRoom,
                            Owner = owner
                        });
                    }
                    else
                    {
                        // Xử lý trường hợp không có chủ sở hữu (nếu cần)
                        classRoomViewModels.Add(new ClassRoomViewModel { ClassRoom = classRoom, Owner = new CustomUser { UserName = "Unknown" } });
                    }
                }
            }

            // Truyền danh sách lớp học của người dùng vào View
            return View(classRoomViewModels);
        }



        // GET: ClassRooms/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRoom = await _context.classRooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classRoom == null)
            {
                return NotFound();
            }

			// Truy vấn thông tin của chủ sở hữu từ bảng người dùng
			var currentUser = await _userManager.GetUserAsync(User);
			var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == classRoom.UserId);
            var Lecture = await _context.BaiGiang.Where(p => p.ClassId == classRoom.Id).ToListAsync();
            if (owner == null)
            {
                return NotFound();
            }
            var viewModel = new ClassRoomViewModel();
            // Kiểm tra null cho owner trước khi thêm vào classRoomViewModels
            if (owner != null)
            {
                viewModel.ClassRoom = classRoom;
                viewModel.Owner = owner;
            }
            else
            {
                // Xử lý trường hợp không có chủ sở hữu (nếu cần)
                viewModel.ClassRoom = classRoom;
                viewModel.Owner = new CustomUser { UserName = "Unknown" };
            }
            foreach (var item in Lecture)
            {
                viewModel.Unit = Lecture;
            }
            if (owner.Id == currentUser.Id)
            {
                viewModel.isOwner = true;
            }
            else
            {
				viewModel.isOwner = false;
			}

            return View(viewModel);
        }


        // GET: ClassRooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ClassRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassRoom classRoom)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    classRoom.UserId = currentUser.Id;
                    _context.Add(classRoom);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            return View(classRoom);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile pdfFile, string lectureName, string ClassId)
        {
            // Kiểm tra xem tệp đã được tải lên có tồn tại không và có rỗng không
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Không có tệp được chọn hoặc tệp trống.");
            }

            // Kiểm tra xem tệp có phải là tệp PDF không
            if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
            {
                return BadRequest("Vui lòng chỉ tải lên tệp PDF.");
            }

            // Đặt vị trí lưu trữ trên máy chủ cho tệp PDF
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "BAIGIANG");

            var fileName = $"{lectureName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu trữ tệp PDF lên đường dẫn đã chỉ định
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            BaiGiang newLecture = new BaiGiang();
            newLecture.UrlBaiGiang = fileName;
            newLecture.Name = lectureName;
            newLecture.ClassId = ClassId;
            _context.BaiGiang.Add(newLecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ClassRoom", new { id = ClassId, tab = "posts" });
        }
    }

}
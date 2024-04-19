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
using System.Text;
using Microsoft.AspNetCore.SignalR;

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
		static List<ClassRoom> userClasses;
		// GET: ClassRooms
		public async Task<IActionResult> Index()
		{
			var currentUser = await _userManager.GetUserAsync(User);
			List<ClassRoomViewModel> classRoomViewModels = new List<ClassRoomViewModel>();
			

			if (currentUser != null)
			{
				// Lấy danh sách lớp học mà người dùng là chủ sở hữu từ bảng classRooms
				userClasses = await _context.classRooms.Where(p => p.UserId == currentUser.Id).ToListAsync();
				// Lấy danh sách lớp học mà người dùng có ID trong bảng ClassroomDetail
				var classDetailClasses = await _context.classroomDetail.Where(p => p.UserId.Trim() == currentUser.Id.Trim()).Select(p => p.ClassRoomId).ToListAsync();

				if (userClasses == null)
				{
					userClasses = await _context.classRooms
						.Where(p => classDetailClasses.Contains(p.Id))
						.ToListAsync();
				}
				else
				{
					var userClasses1 = await _context.classRooms
						.Where(p => classDetailClasses.Contains(p.Id))
						.ToListAsync();
					userClasses.AddRange(userClasses1);
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
			ViewBag.ListRoom = userClasses;
			// Truyền danh sách lớp học của người dùng vào View
			return View(classRoomViewModels);
		}

		

		// GET: ClassRooms/Details/5
		public async Task<IActionResult> Details(string id)
		{
			ViewBag.ListRoom = userClasses;
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
			var chatHistory = await _context.Messages.Where(p => p.ClassRoomId == classRoom.Id).ToListAsync();
			var homework = await _context.baiTaps.Where(p => p.ClassRoomId == classRoom.Id).ToListAsync();
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
			viewModel.Homework = homework;
			viewModel.Message = chatHistory;
			ViewBag.ListRoom = userClasses;

			return View(viewModel);
		}


		// GET: ClassRooms/Create
		public async Task<IActionResult> Create()
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
						.Select(p => p.ClassRoomId)
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
		string GenerateUniqueRandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			StringBuilder randomString = new StringBuilder();
			Random random = new Random();

			while (randomString.Length < length)
			{
				randomString.Append(chars[random.Next(chars.Length)]);
			}

			return randomString.ToString();
		}

		// POST: ClassRooms/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ClassRoom classRoom)
		{
			ModelState.Remove("Id");
			if (ModelState.IsValid)
			{
				var currentUser = await _userManager.GetUserAsync(User);
				if (currentUser != null)
				{
					classRoom.Id = GenerateUniqueRandomString(6);
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
			
			if (pdfFile == null || pdfFile.Length == 0)
			{
				return BadRequest("Không có tệp được chọn hoặc tệp trống.");
			}

			if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
			{
				return BadRequest("Vui lòng chỉ tải lên tệp PDF.");
			}

			var uploadsFolder = Path.Combine(_environment.WebRootPath, "BAIGIANG");

			var fileName = $"{lectureName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
			var filePath = Path.Combine(uploadsFolder, fileName);

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

			return Ok();
		}

		[HttpGet]
		public PartialViewResult GetLecture(string ClassId)
		{
			// Lấy danh sách bài giảng dựa trên ClassId từ cơ sở dữ liệu
			List<BaiGiang> lectures = _context.BaiGiang.Where(l => l.ClassId == ClassId).ToList();

			return PartialView("_lecturePartial", lectures);
		}


		[HttpGet]
		public PartialViewResult GetMessages()
		{
			List<Message> messages = _context.Messages.ToList();

			return PartialView("_MessagePartial", messages);
		}
		[HttpPost]
		public async Task<IActionResult> JoinClassV1(ClassroomDetail classroom)
		{
			ModelState.Remove("UserId");
			ModelState.Remove("RoleId");
			if (ModelState.IsValid)
			{
				var currentUser = await _userManager.GetUserAsync(User);
				if (currentUser != null)
				{
					classroom.UserId = currentUser.Id;
					classroom.RoleId = "Student";
					_context.Add(classroom);
					await _context.SaveChangesAsync();

					return RedirectToAction(nameof(Index));
				}
			}
			return Content("Lỗi");
		}

		[HttpPost]
		public async Task<IActionResult> CreateBaitap(IFormFile? FileUpLoad, string Content, string Title, string ClassId, string FileFormat)
		{
			ModelState.Remove("FileUpLoad");
			if (ModelState.IsValid)
			{
				BaiTap baitap = new BaiTap();
				if (FileUpLoad != null && FileUpLoad.Length > 0)
				{
					var uploadsFolder = Path.Combine(_environment.WebRootPath, "BAITAP");
					// Kiểm tra xem thư mục tồn tại hay không
					if (!Directory.Exists(uploadsFolder))
					{
						// Nếu thư mục không tồn tại, tạo thư mục mới
						Directory.CreateDirectory(uploadsFolder);
					}
					var filePath = Path.Combine(uploadsFolder, FileUpLoad.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await FileUpLoad.CopyToAsync(stream);
					}
					baitap.attractUrl = FileUpLoad.FileName;
				}
				else
				{
					baitap.attractUrl = null;
				}
				
				baitap.Title = Title;
				baitap.Content = Content;
				baitap.Id = Guid.NewGuid().ToString();
				baitap.ClassRoomId = ClassId;
				baitap.FileFormat = FileFormat;

				_context.Add(baitap);
				await _context.SaveChangesAsync();

				return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
			}
			return Redirect("https://hutech.edu.vn");
		}
		[HttpPost]
		public async Task<IActionResult> Nopbai(IFormFile FileNopbai, string ClassId, string BaitapId, DateTime SubmittedAt)
		{
			//ModelState.Remove("FileNopbai");
			if (ModelState.IsValid)
			{
				if (FileNopbai != null && FileNopbai.Length > 0)
				{
					var uploadsFolder = Path.Combine(_environment.WebRootPath, "BAINOP");
					// Kiểm tra xem thư mục tồn tại hay không
					if (!Directory.Exists(uploadsFolder))
					{
						// Nếu thư mục không tồn tại, tạo thư mục mới
						Directory.CreateDirectory(uploadsFolder);
					}
					var filePath = Path.Combine(uploadsFolder, FileNopbai.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await FileNopbai.CopyToAsync(stream);
					}
				}
				var currentuser = await _userManager.GetUserAsync(User);
				BaiNop baiNop = new BaiNop();
				baiNop.ClassId = ClassId;
				baiNop.BaiTapId = BaitapId;
				baiNop.UserId = currentuser.Id;
				baiNop.SubmittedAt = DateTime.Now;
				baiNop.Urlbainop = FileNopbai.FileName;

				_context.Add(baiNop);
				await _context.SaveChangesAsync();
				return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
			}
			return Redirect("https://hutech.edu.vn");
		}

		public async Task<IActionResult> GetAllHomeWork()
		{
            ViewBag.ListRoom = userClasses;
            var lop = userClasses.Select(p => p.Id).ToList();

			var HW = await _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId)).ToListAsync();
			return View(HW);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var classRoom = await _context.classRooms.FindAsync(id);
			if (classRoom == null)
			{
				return NotFound();
			}
			_context.classRooms.Remove(classRoom);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
		// GET: ClassRooms/Edit/5
		public async Task<IActionResult> Edit(string id)
		{
			ViewBag.ListRoom = userClasses;
			if (id == null)
			{
				return NotFound();
			}

			var classRoom = await _context.classRooms.FindAsync(id);
			if (classRoom == null)
			{
				return NotFound();
			}

			return View(classRoom);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, ClassRoom classRoom)
		{
            
            if (id == null || classRoom == null || classRoom.Id != id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Lấy thông tin classRoom hiện tại từ cơ sở dữ liệu
					var existingClassRoom = await _context.classRooms.FindAsync(id);

					if (existingClassRoom == null)
					{
						return NotFound();
					}

					// Lấy thông tin chủ sở hữu (owner) của classRoom hiện tại
					var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == existingClassRoom.UserId);

					// Cập nhật thông tin từ classRoom được gửi từ view
					existingClassRoom.Name = classRoom.Name;
					existingClassRoom.Description = classRoom.Description;

					// Gán lại giá trị UserId
					existingClassRoom.UserId = existingClassRoom.UserId; // Giữ nguyên giá trị UserId

					_context.Update(existingClassRoom);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ClassRoomExists(classRoom.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
			}
			return View(classRoom);
		}

		private bool ClassRoomExists(string id)
		{
			return _context.classRooms.Any(e => e.Id == id);
		}
	}
}
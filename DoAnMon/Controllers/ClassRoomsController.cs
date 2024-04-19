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
using OfficeOpenXml;
using Microsoft.AspNetCore.Hosting;

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
					classRoom.RoomOnline = GetLink();
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

		private string GetLink()
		{
			// Đường dẫn đến tập tin Excel
			string filePath = Path.Combine(_environment.WebRootPath, "Link meeting.xlsx");


			// Mở tập tin Excel bằng thư viện EPPlus
			using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
			{
				// Lấy sheet đầu tiên từ tập tin Excel
				ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

				// Số hàng có dữ liệu trong sheet
				int rowCount = worksheet.Dimension.Rows;

				// List để lưu trữ dữ liệu của mỗi dòng
				List<string> rows = new List<string>();

				// Đọc dữ liệu từ sheet và lưu vào list rows
				for (int row = 1; row <= rowCount; row++)
				{
					string rowData = "";
					for (int col = 1; col <= worksheet.Dimension.Columns; col++)
					{
						if (worksheet.Cells[row, col].Value != null)
						{
							rowData += worksheet.Cells[row, col].Value.ToString() + ",";
						}
					}
					rows.Add(rowData.TrimEnd(','));
				}

				// Lấy ngẫu nhiên một dòng từ list rows
				Random random = new Random();
				int randomIndex = random.Next(0, rows.Count);
				string randomRow = rows[randomIndex];
				return randomRow;
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateBaitap(IFormFile FileUpLoad, string Content, string Title, string ClassId, string FileFormat)
		{

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
			}
			BaiTap baitap = new BaiTap();
			baitap.Title = Title;
			baitap.Content = Content;
			baitap.Id = Guid.NewGuid().ToString();
			baitap.attractUrl = (FileUpLoad != null && FileUpLoad.Length > 0) ? FileUpLoad.FileName : null; ;
			baitap.ClassRoomId = ClassId;
			baitap.FileFormat = FileFormat;

			_context.Add(baitap);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
		}
		[HttpPost]
		public async Task<IActionResult> Nopbai(IFormFile FileNopbai, string ClassId, string BaitapId, DateTime SubmittedAt)
		{

			//ModelState.Remove("FileNopbai");
			if (ModelState.IsValid)
			{
				var currentuser = await _userManager.GetUserAsync(User);
				var MSSV = currentuser.UserName;
				var Name = currentuser.Name;
				var homework = _context.baiTaps.FirstOrDefault(h => h.Id == BaitapId);
				var Tenbai = homework.Title;
				var filename = $"{MSSV}_{Name}_{Tenbai}{Path.GetExtension(FileNopbai.FileName)}";
				var uploadsFolder = Path.Combine(_environment.WebRootPath, "BAINOP");
				if (FileNopbai != null || FileNopbai.Length > 0)
				{

					// Kiểm tra xem thư mục tồn tại hay không
					if (!Directory.Exists(uploadsFolder))
					{
						// Nếu thư mục không tồn tại, tạo thư mục mới
						Directory.CreateDirectory(uploadsFolder);
					}

					var filePath = Path.Combine(uploadsFolder, filename);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await FileNopbai.CopyToAsync(stream);
					}
				}

				if (homework != null)
				{
					homework.HasSubmittedFile = true;
					_context.Update(homework);
					await _context.SaveChangesAsync();
				}
				//var currentuser = await _userManager.GetUserAsync(User);
				BaiNop baiNop = new BaiNop();
				baiNop.ClassId = ClassId;
				baiNop.BaiTapId = BaitapId;
				baiNop.UserId = currentuser.Id;
				baiNop.SubmittedAt = DateTime.Now;
				baiNop.Urlbainop = filename;

				_context.Add(baiNop);
				await _context.SaveChangesAsync();
				return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
			}
			return Redirect("/ClassRooms");
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
			var lectures = await _context.BaiGiang.Where(p => p.ClassId == id).ToListAsync();
			foreach(var item in lectures)
			{
				_context.BaiGiang.Remove(item);
			}
			var homeworks = await _context.baiTaps.Where(p => p.ClassRoomId == id).ToListAsync();
			foreach (var item in homeworks)
			{
				_context.baiTaps.Remove(item);
			}
			var bainops = await _context.BaiNop.Where(p => p.ClassId == id).ToListAsync();
			foreach (var item in bainops)
			{
				_context.BaiNop.Remove(item);
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

		private static string classID;
		public IActionResult AddListSV(string id)
		{
            ViewBag.ListRoom = userClasses;
            classID = id;

            return View();
		}

        [HttpPost]
        public async Task<IActionResult> ImportDataFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length <= 0)
            {
                ViewBag.Message = "Please select a file to upload.";
                return View("Upload");
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "EXCEL");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, excelFile.FileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                excelFile.CopyTo(stream);
            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
					var username = worksheet.Cells[row, 1].Value.ToString();
					var user = await _userManager.FindByNameAsync(username);
					if (user != null)
					{
						// Đọc dữ liệu từ mỗi hàng và tạo đối tượng Employee
						ClassroomDetail student = new ClassroomDetail
						{
							UserId = user.Id,
							ClassRoomId = classID,
							RoleId = "Student"
						};
						// Thêm đối tượng Employee vào cơ sở dữ liệu
						_context.classroomDetail.Add(student);
					}
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
            }

            ViewBag.Message = "File uploaded and data imported successfully.";
            return RedirectToAction("Details", "ClassRooms", new { id = classID });
        }
    }
}
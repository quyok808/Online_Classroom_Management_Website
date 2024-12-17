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
using System.IO.Compression;
using Newtonsoft.Json;
using SQLitePCL;
using DoAnMon.Pagination;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using DoAnMon.ModelListSVDownload;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Security.Cryptography;
using System.Runtime.ConstrainedExecution;
using Microsoft.IdentityModel.Tokens;
using DoAnMon.Migrations;
using DoAnMon.SendMail;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata;
using DoAnMon.SignalR;
using System.Text.RegularExpressions;
using DoAnMon.ViewModels;
using Humanizer;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Drawing;
namespace DoAnMon.Controllers
{
	[Authorize(Roles ="Admin, Teacher, Student")]
	public class ClassRoomsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;
		private readonly IWebHostEnvironment _environment;
		private readonly IStudent _studentRepo;
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private readonly ILogger<HomeController> _logger;
		private readonly Mail _mailService;

		public ClassRoomsController(ApplicationDbContext context, UserManager<CustomUser> userManager, IWebHostEnvironment environment, IStudent studentRepo, ILogger<HomeController> logger, Mail mailService)
		{
			_context = context;
			_userManager = userManager;
			_environment = environment;
			_studentRepo = studentRepo;
			_logger = logger;
			_mailService = mailService;
		}
        public static List<ClassRoom>? userClasses;
		// GET: ClassRooms
		public async Task<IActionResult> Index()
		{
			List<ClassRoom> classroomsList = _context.classRooms.ToList();
			DateTime nowDate = DateTime.UtcNow;
			foreach (var item in classroomsList)
			{
				if (item.EndDate.Date.AddDays(30) == nowDate.Date)
				{
                    DeleteClassrooms(item.Id);
                }
			}
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

				userClasses = userClasses.OrderBy(p => p.STT).ToList();
				foreach (var classRoom in userClasses)
				{
					var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == classRoom.UserId);

					// Kiểm tra null cho owner trước khi thêm vào classRoomViewModels
					if (owner != null)
					{
						classRoomViewModels.Add(new ClassRoomViewModel
						{
							ClassRoom = classRoom,
							Owner = owner,
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
            ViewBag.listNotiNew = await _context.FriendRequests.Where(user => user.TargetId.Equals(currentUser.Id) && user.IsAccepted == false).OrderByDescending(p => p.createAt).CountAsync();
            return View(classRoomViewModels);
		}

        public void DeleteClassrooms(string id)
        {
            var classRoom = _context.classRooms.FirstOrDefault(p => p.Id.Equals(id));
            if (classRoom == null)
            {
				return;
            }
            var lectures = _context.BaiGiang.Where(p => p.ClassId.Equals(id)).ToList();


            foreach (var item in lectures)
            {
                deleteFile("Uploads/BAIGIANG", item.UrlBaiGiang);

                _context.BaiGiang.Remove(item);
            }
            var homeworks = _context.baiTaps.Where(p => p.ClassRoomId.Equals(id)).ToList();
            foreach (var item in homeworks)
            {

                deleteFile("Uploads/BAITAP", item.attractUrl);

                _context.baiTaps.Remove(item);
            }
            var bainops = _context.BaiNop.Where(p => p.ClassId.Equals(id)).ToList();
            foreach (var item in bainops)
            {

                deleteFile("Uploads/BAINOP", item.Urlbainop);

                _context.BaiNop.Remove(item);
            }
            List<ClassroomDetail> classroomDetails = _context.classroomDetail.Where(p => p.ClassRoomId.Equals(id)).ToList();
            foreach (var item in classroomDetails)
            {
                _context.classroomDetail.Remove(item);
            }
			var rubric = _context.Rubric.FirstOrDefault(p => p.ClassRoomId.Equals(id));
			if (rubric != null)
			{
				var Criteria = _context.Criteria.Where(p => p.RubricId == rubric.Id);
				Criteria.ForEachAsync(p => _context.Criteria.Remove(p));
				var StudentsList = _context.Students.Where(p => p.RubricId == rubric.Id);
				StudentsList.ForEachAsync(p => _context.Students.Remove(p));
				_context.Rubric.Remove(rubric);
			}

			var diemdanh = _context.diemDanh.Where(p => p.ClassRoomId.Equals(id)).ToList();
            foreach (var item in diemdanh) 
            {
                _context.diemDanh.Remove(item);
            }

            _context.classRooms.Remove(classRoom);
            _context.SaveChanges();
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
			var homework = await _context.baiTaps.Where(p => p.ClassRoomId == classRoom.Id).OrderByDescending(p => p.CreatedAt).ToListAsync();
			var post = await _context.posts.Where(p => p.ClassRoomId == classRoom.Id).ToListAsync();
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
                ViewBag.Isowner = true;
            }
            else
            {
                viewModel.isOwner = false;
                ViewBag.Isowner = false;
            }
			viewModel.Post = post;
            viewModel.Homework = homework;
			viewModel.Message = chatHistory;
			ViewBag.ListRoom = userClasses;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy UserId
			var userposts = await _context.posts
				.Where(p => p.UserId == userId) // Lọc theo UserId
				.GroupBy(p => new { p.Title, p.Content })
				.Select(g => g.First())
				.ToListAsync();
			ViewBag.UserPosts = userposts;
			ViewBag.UserId = userId;

			var reusableAssignments = await _context.baiTaps
				.Where(bt => _context.classRooms
				.Any(cr => cr.Id == bt.ClassRoomId && cr.UserId == userId))
				.GroupBy(bt => new { bt.Title, bt.Content})
				.Select(g => g.First())
				.ToListAsync();
			ViewBag.ReuseBaitap = reusableAssignments;

			var reusableLectures = await _context.BaiGiang
				.Where(bt => _context.classRooms
				.Any(cr => cr.Id == bt.ClassId && cr.UserId == userId))
				.GroupBy(bt => new { bt.Name, bt.UrlBaiGiang })
				.Select(g => g.First())
				.ToListAsync();
			ViewBag.ReuseBaigiang = reusableLectures;

			var listBT = await _context.baiTaps.Where(p => p.ClassRoomId == id).ToListAsync();


			var Diem = new List<DiemViewModel>();

			var listUser = _context.classroomDetail.Where(p => p.ClassRoomId == id).Select(p => p.UserId).ToList();

			var Users = await _context.Users.Where(p => listUser.Contains(p.Id)).ToListAsync();
			
			foreach(var user in Users)
			{
				var newDiem = new DiemViewModel();
				newDiem.MSSV = user.Mssv;
				newDiem.HoVaTen = user.Name;
				var dtb = _context.bangDiem.FirstOrDefault(p => p.UserId == user.Id && p.ClassRoomId == id).DTB;
				var bt = listBT.Select(p => p.Id).ToList();
				// Kiểm tra và khởi tạo listDiemBT nếu chưa tồn tại
				if (newDiem.listDiemBT == null)
				{
					newDiem.listDiemBT = new List<decimal>();
				}
				foreach (var b in bt)
				{
					// Kiểm tra xem có bài nộp nào thỏa mãn điều kiện không
					var baiNop = _context.BaiNop.FirstOrDefault(p => b == p.BaiTapId && p.UserId == user.Id);

					if (baiNop != null)
					{
						// Nếu có, lấy điểm của bài nộp
						decimal? DiemBT = baiNop.Diem;

						if (DiemBT == null)
						{
							// Nếu điểm là null, gán giá trị mặc định
							DiemBT = 0;
						}

						newDiem.listDiemBT.Add((decimal)DiemBT);
					}
					else
					{
						// Nếu không có bài nộp thỏa mãn điều kiện, nhưng vẫn có tồn tại bài tập đó
						// Thêm giá trị mặc định (ví dụ: 0) vào danh sách
						newDiem.listDiemBT.Add((decimal)0);
					}
				}
				newDiem.DTB = (decimal)dtb;
				Diem.Add(newDiem);
			}
			ViewBag.ListBT = listBT;
			ViewBag.ListDiem = Diem;

			viewModel.ClassDates = CalculateClassDates(
										classRoom.StartDate, 
										classRoom.EndDate, 
										classRoom.DaysOfWeek, 
										new TimeSpan(classRoom.StartTime.Hours, classRoom.StartTime.Minutes, 0),
										new TimeSpan(classRoom.EndTime.Hours, classRoom.EndTime.Minutes,0)
										);

			
			if (classRoom.RubricId == -1)
			{
				viewModel.CustomRubric = false;
			}
			else
			{
				viewModel.CustomRubric = true;
				viewModel.Rubric = await _context.Rubric.FirstOrDefaultAsync(p => p.ClassRoomId.Equals(id));
			}

			var userClassDetails = _context.classroomDetail.FirstOrDefault(p => p.ClassRoomId.Equals(id) && p.UserId.Equals(currentUser.Id));
            if (userClassDetails != null && userClassDetails.GroupId != null)
			{
                List<ClassroomDetail> listUserGroup = await _context.classroomDetail.Where(p => p.GroupId.Equals(userClassDetails.GroupId) && p.ClassRoomId.Equals(id)).ToListAsync();
                ViewBag.ListUserGroup = listUserGroup;
            }
            
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
				userClasses = userClasses.OrderBy(p => p.STT).ToList();
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
		public async Task<IActionResult> Create(ClassRoom classRoom, string[] DaysOfWeek, bool haveRubric)
		{
			ModelState.Remove("Id");
			if (ModelState.IsValid)
			{
				var currentUser = await _userManager.GetUserAsync(User);
				if (currentUser != null)
				{
					classRoom.Id = GenerateUniqueRandomString(6);
					classRoom.UserId = currentUser.Id;
					classRoom.RoomOnline = "https://meeting-room-onlya1.glitch.me?room=" + linkRoom;
					classRoom.backgroundUrl = GetBackgroundFile();
					classRoom.STT = 0;
                    if (haveRubric)
					{
						classRoom.RubricId = -1;
					}

                    // Ghép các ngày học lại thành chuỗi
                    classRoom.DaysOfWeek = string.Join(",", DaysOfWeek);

					_context.Add(classRoom);
					await _context.SaveChangesAsync();

					var classrooms = await _context.classRooms.ToListAsync();
					foreach (var clr in classrooms)
					{
						clr.STT++;
					}
					await _context.SaveChangesAsync();
					if (haveRubric)
						return RedirectToAction(nameof(Index));
					return RedirectToAction("Create", "Rubrics", new { classroomId = classRoom.Id });
				}
			}
			return View(classRoom);
		}
		static string linkRoom;

		public string GetBackgroundFile()
		{
			var path = Path.Combine(_environment.WebRootPath, "images");
			string[] files = Directory.GetFiles(path);

			var fileNames = files.Select(file => Path.GetFileName(file))
								 .Where(filename => filename.Contains("classImage_Default"))
								 .ToList();

			if (fileNames.Count == 0)
			{
				return null; // Or some default value if no valid files are found
			}

			Random rand = new Random();
			int number = rand.Next(0, fileNames.Count); // Random number within the valid range
			return fileNames[number];
		}


		[HttpPost]
        public ActionResult ReceiveRoomUrl(string roomUrl1)
        {
			linkRoom = roomUrl1;
            // Trả về kết quả, có thể là JSON hoặc các loại dữ liệu khác
            return Json(new { success = true });
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

		[HttpPost]
		public async Task<IActionResult> ReuseLecture(int originalLectureId, string newLectureName, string classId)
		{
			// Kiểm tra xem bài giảng gốc có tồn tại không
			var originalLecture = await _context.BaiGiang.FindAsync(originalLectureId);
			if (originalLecture == null)
			{
				return NotFound("Bài giảng không tồn tại.");
			}

			// Tạo một bài giảng mới dựa trên bài giảng gốc
			BaiGiang reusedLecture = new BaiGiang
			{
				Name = !string.IsNullOrWhiteSpace(newLectureName) ? newLectureName : originalLecture.Name,
				UrlBaiGiang = originalLecture.UrlBaiGiang, // Dùng lại file URL
				ClassId = classId, // Gán vào lớp học mới
			};

			// Thêm bài giảng mới vào cơ sở dữ liệu
			_context.BaiGiang.Add(reusedLecture);
			await _context.SaveChangesAsync();

			// Chuyển hướng về chi tiết lớp học
			return RedirectToAction("Details", "ClassRooms", new { id = classId });
		}

		[HttpGet]
        public async Task<PartialViewResult> GetLectureAsync(string ClassId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Lấy danh sách bài giảng dựa trên ClassId từ cơ sở dữ liệu
            List<BaiGiang> lectures = _context.BaiGiang.Where(l => l.ClassId == ClassId).ToList();
            ClassRoom temp = _context.classRooms.FirstOrDefault(p => p.Id == ClassId);
            if (temp != null)
            {
                if (currentUser.Id == temp.UserId)
                {
                    ViewBag.Isowner = true;
                }
                else
                {
                    ViewBag.Isowner = false;
                }
            }
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

					var newDiemRecord = new BangDiem
					{
						UserId = currentUser.Id,
						ClassRoomId = classroom.ClassRoomId,
						DTB = 0
					};
					 _context.bangDiem.Add(newDiemRecord);
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
		public async Task<IActionResult> CreateBaitap(string AttactURL, string Content, string Title, string ClassId, string FileFormat, DateTime Deadline, DateTime CreateAt, int MaxSize, string bt_category, string showMode)
		{

			BaiTap baitap = new BaiTap();
			baitap.Title = Title;
			baitap.Content = Content;
			baitap.Id = Guid.NewGuid().ToString();
			baitap.attractUrl = AttactURL;
			baitap.ClassRoomId = ClassId;
			baitap.ShowMode = showMode;
			baitap.CreatedAt = DateTime.Now;
			baitap.FileFormat = FileFormat;
			baitap.MaxSize = MaxSize != 0 ? MaxSize : 10;
			if (Deadline.ToString() != "01/01/0001 12:00:00 AM")
			{
				baitap.Deadline = Deadline;
			}
			else
			{
				baitap.Deadline = null;
			}
			baitap.Loaibt = bt_category;
			_context.Add(baitap);
			await _context.SaveChangesAsync();
			List<CustomUser>? listUserId = await _context.classroomDetail.Where(p => p.ClassRoomId.Equals(ClassId)).Select(p => p.User).ToListAsync();
			List<CustomUser>? userMssvChan = new List<CustomUser>();
			List<CustomUser>? userMssvLe = new List<CustomUser>();

			foreach (var item in listUserId)
			{
				string s = item.Mssv;
				int total = 0;

				// Lấy 4 ký tự cuối
				string lastFour = s.Substring(s.Length - 4);

				// Chuyển đổi thành số
				int number = int.Parse(lastFour);

				total += number;

				if (total % 2 == 0)
				{
					userMssvChan.Add(item);
				}
				else
				{
					userMssvLe.Add(item);
				}
			}
			if (baitap.ShowMode.Equals("Chan"))
			{
				listUserId.Clear();
				listUserId = userMssvChan;
			} else if (baitap.ShowMode.Equals("Le"))
			{
				listUserId.Clear();
				listUserId = userMssvLe;
			}


			// Gửi email thông báo
			const int batchSize = 50; // Số lượng email gửi trong một batch

			// Chia danh sách người dùng thành từng lô
			var emailBatches = listUserId.Select((user, index) => new { user, index })
				.GroupBy(x => x.index / batchSize)
				.Select(g => g.Select(x => x.user).ToList())
				.ToList();

			foreach (var batch in emailBatches)
			{
				try
				{
					// Tạo danh sách email trong batch
					var emails = batch.Select(user => user.Email).ToList();

					// Gửi email cho từng người trong batch
					foreach (var user in batch)
					{
						string email = user.Email;
						string subject = $"Bài tập mới: {baitap.Title}";
						string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            font-family: 'JetBrains Mono', serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border: 1px solid #dddddd;
                            border-radius: 8px;
                            overflow: hidden;
                        }}
                        .header {{
                            background-color: #007bff;
                            color: #ffffff;
                            text-align: center;
                            padding: 20px;
                            font-family: 'Rowdies', serif;
                        }}
                        .content {{
                            padding: 20px;
                            color: #333333;
                        }}
                        .content h1 {{
                            font-size: 24px;
                            margin-bottom: 10px;
                        }}
                        .content p {{
                            font-size: 16px;
                            line-height: 1.5;
                        }}
                        .footer {{
                            background-color: #f4f4f4;
                            color: #888888;
                            text-align: center;
                            padding: 10px;
                            font-size: 12px;
                        }}
                        .deadline {{
                            color: #e74c3c;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h1>THÔNG BÁO BÀI TẬP</h1>
                        </div>
                        <div class='content'>
                            <h1>{baitap.Title}</h1>
                            <p>{baitap.Content}</p>
                            <p>Hạn nộp: <span class='deadline'>{(string.IsNullOrEmpty(baitap.Deadline?.ToString("dd/MM/yyyy HH:mm")) ? "Vô thời hạn" : baitap.Deadline?.ToString("dd/MM/yyyy HH:mm"))}</span></p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý lớp học trực tuyến OnlyA.</p>
                        </div>
                    </div>
                </body>
                </html>";
						await _mailService.SendEmailAsync(email, subject, body);
					}
				}
				catch (Exception ex)
				{
					// Ghi log lỗi cho từng batch
					Console.WriteLine($"Lỗi khi gửi email batch: {ex.Message}");
				}
			}

			await TinhDTBAsync(ClassId);
			return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
		}

		[HttpPost]
		public async Task<IActionResult> ReuseBaitap(string originalBaitapId, string newTitle, string newContent, DateTime? newDeadline, string classId, string newshowMode)
		{
			// Tìm bài tập gốc
			var originalBaitap = await _context.baiTaps.FindAsync(originalBaitapId);
			if (originalBaitap == null)
			{
				return NotFound("Bài tập không tồn tại.");
			}

			// Tạo bài tập mới từ bài tập gốc
			BaiTap newBaitap = new BaiTap
			{
				Id = Guid.NewGuid().ToString(),
				Title = string.IsNullOrWhiteSpace(newTitle) ? originalBaitap.Title : newTitle,
				Content = string.IsNullOrWhiteSpace(newContent) ? originalBaitap.Content : newContent,
				attractUrl = originalBaitap.attractUrl,
				ClassRoomId = classId ?? originalBaitap.ClassRoomId,
				FileFormat = originalBaitap.FileFormat,
				MaxSize = originalBaitap.MaxSize,
				Loaibt = originalBaitap.Loaibt,
				ShowMode = newshowMode,
				CreatedAt = DateTime.Now,
				Deadline = newDeadline ?? originalBaitap.Deadline
			};

			// Thêm bài tập mới vào database
			_context.Add(newBaitap);
			await _context.SaveChangesAsync();

			List<CustomUser>? listUserId = await _context.classroomDetail.Where(p => p.ClassRoomId.Equals(classId)).Select(p => p.User).ToListAsync();
			List<CustomUser>? userMssvChan = new List<CustomUser>();
			List<CustomUser>? userMssvLe = new List<CustomUser>();

			foreach (var item in listUserId)
			{
				string s = item.Mssv;
				int total = 0;

				// Lấy 4 ký tự cuối
				string lastFour = s.Substring(s.Length - 4);

				// Chuyển đổi thành số
				int number = int.Parse(lastFour);

				total += number;

				if (total % 2 == 0)
				{
					userMssvChan.Add(item);
				}
				else
				{
					userMssvLe.Add(item);
				}
			}
			if (newBaitap.ShowMode.Equals("Chan"))
			{
				listUserId.Clear();
				listUserId = userMssvChan;
			}
			else if (newBaitap.ShowMode.Equals("Le"))
			{
				listUserId.Clear();
				listUserId = userMssvLe;
			}


			// Gửi email thông báo
			const int batchSize = 50; // Số lượng email gửi trong một batch

			// Chia danh sách người dùng thành từng lô
			var emailBatches = listUserId.Select((user, index) => new { user, index })
				.GroupBy(x => x.index / batchSize)
				.Select(g => g.Select(x => x.user).ToList())
				.ToList();

			foreach (var batch in emailBatches)
			{
				try
				{
					// Tạo danh sách email trong batch
					var emails = batch.Select(user => user.Email).ToList();

					// Gửi email cho từng người trong batch
					foreach (var user in batch)
					{
						string email = user.Email;
						string subject = $"Bài tập mới: {newBaitap.Title}";
						string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            font-family: 'JetBrains Mono', serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border: 1px solid #dddddd;
                            border-radius: 8px;
                            overflow: hidden;
                        }}
                        .header {{
                            background-color: #007bff;
                            color: #ffffff;
                            text-align: center;
                            padding: 20px;
                            font-family: 'Rowdies', serif;
                        }}
                        .content {{
                            padding: 20px;
                            color: #333333;
                        }}
                        .content h1 {{
                            font-size: 24px;
                            margin-bottom: 10px;
                        }}
                        .content p {{
                            font-size: 16px;
                            line-height: 1.5;
                        }}
                        .footer {{
                            background-color: #f4f4f4;
                            color: #888888;
                            text-align: center;
                            padding: 10px;
                            font-size: 12px;
                        }}
                        .deadline {{
                            color: #e74c3c;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h1>THÔNG BÁO BÀI TẬP</h1>
                        </div>
                        <div class='content'>
                            <h1>{newBaitap.Title}</h1>
                            <p>{newBaitap.Content}</p>
                            <p>Hạn nộp: <span class='deadline'>{(string.IsNullOrEmpty(newBaitap.Deadline?.ToString("dd/MM/yyyy HH:mm")) ? "Vô thời hạn" : newBaitap.Deadline?.ToString("dd/MM/yyyy HH:mm"))}</span></p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động từ hệ thống quản lý lớp học trực tuyến OnlyA.</p>
                        </div>
                    </div>
                </body>
                </html>";
						await _mailService.SendEmailAsync(email, subject, body);
					}
				}
				catch (Exception ex)
				{
					// Ghi log lỗi cho từng batch
					Console.WriteLine($"Lỗi khi gửi email batch: {ex.Message}");
				}
			}


			// Gọi hàm tính điểm trung bình (nếu cần)
			await TinhDTBAsync(classId ?? originalBaitap.ClassRoomId);

			// Redirect về trang chi tiết lớp học
			return RedirectToAction("Details", "ClassRooms", new { id = classId ?? originalBaitap.ClassRoomId });
		}

		[HttpPost]
		public async Task<IActionResult> CreatePost(string Content, string Title, string ClassId, DateTime CreateTime, string UserId)
		{
			Post posts = new Post();
			posts.Id = Guid.NewGuid().ToString();
			posts.Title = Title;
			posts.Content = Content;
			posts.CreateTime = DateTime.Now;
			posts.ClassRoomId = ClassId;
			posts.UserId = UserId;
			_context.Add(posts);
			await _context.SaveChangesAsync();
			return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
		}
		[HttpPost]
		public async Task<IActionResult> ReusePost(string postId, string ClassId, string UserId, string Title, string Content)
		{
			var post = await _context.posts.FindAsync(postId);
			if (post == null)
			{
				return NotFound();
			}

			// Tạo bài post mới dựa trên bài post đã chọn
			var newPost = new Post
			{
				Id = Guid.NewGuid().ToString(), // Tạo Id mới cho bài post
				Title = string.IsNullOrWhiteSpace(Title) ? post.Title : Title, // Sử dụng tiêu đề đã chỉnh sửa hoặc tiêu đề cũ nếu trống
				Content = string.IsNullOrWhiteSpace(Content) ? post.Content : Content, // Sử dụng nội dung đã chỉnh sửa hoặc nội dung cũ nếu trống
				CreateTime = DateTime.Now, // Thiết lập thời gian tạo
				ClassRoomId = ClassId, // Thiết lập ID của Classroom
				UserId = UserId // Lấy UserId từ người dùng hiện tại
			};

			_context.posts.Add(newPost);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
		}
		private async Task TinhDTBAsync(string classId)
		{
			// Lấy danh sách userId trong lớp học
			var listUser = await _context.bangDiem
				.Where(p => p.ClassRoomId == classId)
				.Select(p => p.UserId)
				.ToListAsync();

			// Đếm số lượng bài tập
			int tongslBT = await _context.baiTaps
				.Where(p => p.ClassRoomId.Trim() == classId && p.ShowMode.Equals("All"))
				.CountAsync();

			// Lặp qua từng người dùng và tính tổng điểm
			foreach (var userId in listUser)
			{
				var user = await _userManager.FindByIdAsync(userId);
				string s = user.Mssv;
				int total = 0;

				// Lấy 4 ký tự cuối
				string lastFour = s.Substring(s.Length - 4);

				// Chuyển đổi thành số
				int number = int.Parse(lastFour);
				total += number;

				if (total % 2 == 0)
				{
					int tongslBTChan = await _context.baiTaps
						.Where(p => p.ClassRoomId.Trim() == classId && p.ShowMode.Equals("Chan"))
						.CountAsync();
					tongslBT += tongslBTChan;
				}
				else
				{
					int tongslBTLe = await _context.baiTaps
						.Where(p => p.ClassRoomId.Trim() == classId && p.ShowMode.Equals("Le"))
						.CountAsync();
					tongslBT += tongslBTLe;
				}

				// Lấy tổng điểm của người dùng
				decimal TongDiem_User = (decimal)await _context.BaiNop
					.Where(p => p.UserId.Trim() == userId.Trim() && p.ClassId.Trim() == classId.Trim())
					.Select(p => p.Diem)
					.SumAsync();

				// Lấy bản ghi của người dùng
				var score = await _context.bangDiem
					.FirstOrDefaultAsync(p => p.UserId == userId && p.ClassRoomId == classId);

				if (score != null)
				{
					if (tongslBT > 0)
					{
						score.DTB = ((TongDiem_User / tongslBT) * 0.7m) + (decimal)DiemDD(userId, classId);
					}
					else
					{
						score.DTB = (decimal)DiemDD(userId, classId);
					}
				}
			}

			// Lưu tất cả thay đổi cùng lúc
			await _context.SaveChangesAsync();
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
				string filename = "";
				var usercurrent = _context.classroomDetail.FirstOrDefault(p => p.UserId.Equals(currentuser.Id) && p.ClassRoomId.Equals(ClassId));
				List<ClassroomDetail>? listUser = new List<ClassroomDetail>();
				if (usercurrent.GroupId != null)
				{
					listUser = _context.classroomDetail.Where(p => p.GroupId.Equals(usercurrent.GroupId)).Include(p => p.User).ToList();
				}
				if (homework.Loaibt.Equals("CaNhan"))
				{
					filename = $"{MSSV}_{Name}_{Tenbai}{Path.GetExtension(FileNopbai.FileName)}";
				} else
				{
					string fileusername = "";

					foreach (var item in listUser)
					{
						fileusername += item.User.Name + "_" + item.User.Mssv + "_";
					}

					filename = $"{fileusername}{Tenbai}{Path.GetExtension(FileNopbai.FileName)}";
				}
				var uploadsFolder = Path.Combine(_environment.WebRootPath,"Uploads", "BAINOP");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
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
				if (homework.Loaibt.Equals("CaNhan"))
				{
					BaiNop baiNop = new BaiNop();
					baiNop.ClassId = ClassId;
					baiNop.BaiTapId = BaitapId;
					baiNop.UserId = currentuser.Id;
					baiNop.SubmittedAt = DateTime.Now;
					baiNop.Urlbainop = filename;
					baiNop.Diem = 0;
					_context.Add(baiNop);
				}
				else
				{
                    foreach (var item in listUser)
                    {
						BaiNop baiNop = new BaiNop();
						baiNop.ClassId = ClassId;
						baiNop.BaiTapId = BaitapId;
						baiNop.UserId = item.User.Id;
						baiNop.SubmittedAt = DateTime.Now;
						baiNop.Urlbainop = filename;
						baiNop.Diem = 0;
						_context.Add(baiNop);
					}
                }
				await _context.SaveChangesAsync();
				return RedirectToAction("Details", "ClassRooms", new { id = ClassId });
			}
			return Redirect("/ClassRooms");
		}

		public async Task<IActionResult> GetAllHomeWork(string query, int pageNumber = 1)
		{
			ViewBag.ListRoom = userClasses;
			var lop = userClasses.Select(p => p.Id).ToList();


			int pageSize = 5;
			IQueryable<BaiTap> BaitapsQuery = _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId));
			List<BaiTap> t = _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId)).ToList();
			if (!string.IsNullOrEmpty(query))
			{
				BaitapsQuery = BaitapsQuery.Where(p => p.Title.Contains(query) || p.ClassRoomId.Trim() == query.Trim());
			}
			var paginatedBaiTaps = await PaginatedList<BaiTap>.CreateAsync(BaitapsQuery, pageNumber, pageSize);
			paginatedBaiTaps.CurrentQuery = query;
			ViewBag.Bainop = _context.BaiNop.ToList();
			ViewBag.ClassroomDetail = _context.classroomDetail.ToList();
			return View(paginatedBaiTaps);
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


			foreach (var item in lectures)
			{
				deleteFile("Uploads/BAIGIANG", item.UrlBaiGiang);

				_context.BaiGiang.Remove(item);
			}
			var homeworks = await _context.baiTaps.Where(p => p.ClassRoomId == id).ToListAsync();
			foreach (var item in homeworks)
			{

				deleteFile("Uploads/BAITAP", item.attractUrl);

				_context.baiTaps.Remove(item);
			}
			var bainops = await _context.BaiNop.Where(p => p.ClassId == id).ToListAsync();
			foreach (var item in bainops)
			{

				deleteFile("Uploads/BAINOP", item.Urlbainop);

				_context.BaiNop.Remove(item);
			}
			List<ClassroomDetail> classroomDetails = await _context.classroomDetail.Where(p => p.ClassRoomId.Equals(id)).ToListAsync();
			foreach(var item in classroomDetails)
			{
				_context.classroomDetail.Remove(item);
			}
			var rubric = _context.Rubric.FirstOrDefault(p => p.ClassRoomId.Equals(id));
			if (rubric != null)
			{
				var Criteria = _context.Criteria.Where(p => p.RubricId == rubric.Id);
				await Criteria.ForEachAsync(p => _context.Criteria.Remove(p));
				var StudentsList = _context.Students.Where(p => p.RubricId == rubric.Id);
				await StudentsList.ForEachAsync(p => _context.Students.Remove(p));
				_context.Rubric.Remove(rubric);
			}

            var diemdanh = _context.diemDanh.Where(p => p.ClassRoomId.Equals(id)).ToList();
            foreach (var item in diemdanh)
            {
                _context.diemDanh.Remove(item);
            }
            _context.classRooms.Remove(classRoom);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		//DELETE FILE
		public void deleteFile(string folder, string fileName)
		{
			if (fileName.IsNullOrEmpty())
			{
				return;
			}
			var uploadsFolder = Path.Combine(_environment.WebRootPath, folder);

			var filePath = Path.Combine(uploadsFolder, fileName);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
		}

		// GET: ClassRooms/Edit/5
		public async Task<IActionResult> Edit(string id)
		{
			if (id == null)
			{
				return NotFound();
			}

			// Lấy thông tin lớp học dựa trên Id
			var classRoom = await _context.classRooms.FindAsync(id);
			if (classRoom == null)
			{
				return NotFound();
			}

			// Truyền dữ liệu ra ViewModel
			var viewModel = new EditClassRoomViewModel
			{
				Id = classRoom.Id,
				Name = classRoom.Name,
				Description = classRoom.Description,
				StartDate = classRoom.StartDate,
				EndDate = classRoom.EndDate,
				DaysOfWeek = classRoom.DaysOfWeek?.Split(',') ?? Array.Empty<string>(),
				StartTime = classRoom.StartTime,
				EndTime = classRoom.EndTime
			};

			return View(viewModel);
		}

		// POST: ClassRooms/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit1(string id, EditClassRoomViewModel viewModel)
		{
			if (id != viewModel.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Lấy thông tin lớp học từ database
					var classRoom = await _context.classRooms.FindAsync(id);
					if (classRoom == null)
					{
						return NotFound();
					}

					// Cập nhật các trường được chỉnh sửa
					classRoom.Name = viewModel.Name;
					classRoom.Description = viewModel.Description;
					classRoom.StartDate = viewModel.StartDate;
					classRoom.EndDate = viewModel.EndDate;
					classRoom.DaysOfWeek = string.Join(",", viewModel.DaysOfWeek);
					classRoom.StartTime = viewModel.StartTime;
					classRoom.EndTime = viewModel.EndTime;

					// Lưu lại thay đổi
					_context.Update(classRoom);
					await _context.SaveChangesAsync();

					return RedirectToAction(nameof(Create));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ClassRoomExists(viewModel.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
			}
			return View(viewModel);
		}

		// GET: ClassRooms/Edit/5
		//public async Task<IActionResult> Edit(string id)
		//{
		//	if (string.IsNullOrEmpty(id))
		//	{
		//		return BadRequest();
		//	}

		//	var classRoom = await _context.classRooms.FindAsync(id);
		//	if (classRoom == null)
		//	{
		//		return NotFound();
		//	}

		//	var viewModel = new EditClassRoomViewModel
		//	{
		//		Id = classRoom.Id,
		//		Name = classRoom.Name,
		//		Description = classRoom.Description,
		//		StartDate = classRoom.StartDate,
		//		EndDate = classRoom.EndDate,
		//		DaysOfWeek = classRoom.DaysOfWeek?.Split(',') ?? Array.Empty<string>(),
		//		StartTime = classRoom.StartTime,
		//		EndTime = classRoom.EndTime
		//	};

		//	return PartialView("_EditClassRoom", viewModel);
		//}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, EditClassRoomViewModel viewModel)
		{
			if (id != viewModel.Id)
			{
				return Json(new { success = false, message = "ID không hợp lệ." });
			}

			if (!ModelState.IsValid)
			{
				return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
			}

			try
			{
				var classRoom = await _context.classRooms.FindAsync(id);
				if (classRoom == null)
				{
					return Json(new { success = false, message = "Không tìm thấy lớp học." });
				}

				classRoom.Name = viewModel.Name;
				classRoom.Description = viewModel.Description;
				classRoom.StartDate = viewModel.StartDate;
				classRoom.EndDate = viewModel.EndDate;
				classRoom.DaysOfWeek = string.Join(",", viewModel.DaysOfWeek);
				classRoom.StartTime = viewModel.StartTime;
				classRoom.EndTime = viewModel.EndTime;
				classRoom.ShowRubric = viewModel.ShowRubric;

				_context.Update(classRoom);
				await _context.SaveChangesAsync();

				return Json(new { success = true, message = "Lớp học đã được cập nhật thành công." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi chỉnh sửa lớp học.");
				return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật lớp học." });
			}
		}


		// Kiểm tra lớp học có tồn tại không
		private bool ClassRoomExists(string id)
        {
            return _context.classRooms.Any(e => e.Id == id);
        }

		private static string classID;

		[HttpPost]
		public async Task<IActionResult> ImportDataFromExcel(IFormFile excelFile, string classID)
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

						var newDiemRecord = new BangDiem
						{
							UserId = user.Id,
							ClassRoomId = classID,
							DTB = 0
						};
						_context.bangDiem.Add(newDiemRecord);
					}
				}

				// Lưu thay đổi vào cơ sở dữ liệu
				_context.SaveChanges();
			}

			ViewBag.Message = "File uploaded and data imported successfully.";
			return RedirectToAction("Details", "ClassRooms", new { id = classID });
		}

		public IActionResult GetAllBTStu(string classid, string baitapID)
		{
			ViewBag.ListRoom = userClasses;
			var listbainop = _context.BaiNop.Where(p => p.BaiTapId == baitapID && p.ClassId == classid).ToList();
			return View(listbainop);
		}

		public IActionResult DownloadFiles(string baiNop)
		{
			// Giải mã chuỗi base64
			string baiNopJson = Encoding.UTF8.GetString(Convert.FromBase64String(baiNop));

			// Chuyển chuỗi JSON thành danh sách List<BaiNop>
			List<BaiNop> lbn = JsonConvert.DeserializeObject<List<BaiNop>>(baiNopJson);

			// Danh sách đường dẫn tập tin muốn tải xuống

			var fileNames = new List<string>();
			foreach (var item in lbn)
			{
				fileNames.Add(("Uploads/BAINOP/" + item.Urlbainop));
			}
			string name = lbn[0].ClassId + "_" + lbn[0].BaiTapId;
			// Tạo tên tập tin nén
			string zipFileName = name + ".zip";
			// Đường dẫn lưu tập tin nén trên máy chủ
			string zipFilePath = Path.Combine(_environment.WebRootPath, zipFileName);
			if (System.IO.File.Exists(zipFilePath))
			{
				// Xử lý trường hợp tập tin đã tồn tại (ví dụ: xóa tập tin cũ)
				System.IO.File.Delete(zipFilePath);
			}
			// Nén các tập tin thành tập tin nén
			using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
			{
				foreach (var fileName in fileNames)
				{
					// Thêm tập tin vào tập tin nén
					zipArchive.CreateEntryFromFile(Path.Combine(_environment.WebRootPath, fileName), Path.GetFileName(fileName));
				}
			}

			// Trả về tập tin nén để người dùng tải xuống
			return PhysicalFile(zipFilePath, "application/zip", zipFileName);
		}

		public async Task<IActionResult> GetOwnBTAsync(string classid, string baitapID)
		{
			ViewBag.ListRoom = userClasses;
			var currentUser = await _userManager.GetUserAsync(User);
			var listbainop = _context.BaiNop.Where(p => p.BaiTapId == baitapID && p.ClassId == classid && p.UserId == currentUser.Id).ToList();
			return View("GetAllBTStu", listbainop);
		}

		[HttpPost]
		public async Task<IActionResult> JoinClassWithQRCode(string qrData)
		{
			try
			{
				var currentUser = await _userManager.GetUserAsync(User);
				if (currentUser == null)
				{
					return Json(new { success = false, error = "Người dùng không tồn tại" });
				}
				ClassRoom? find = _context.classRooms.FirstOrDefault(p => p.Id == qrData);
				if (find == null)
				{
					return Json(new { success = false, error = " Mã phòng không tồn tại" });
				}
				// Lưu trữ dữ liệu vào cơ sở dữ liệu
				var newData = new ClassroomDetail
				{
					UserId = currentUser.Id,
					ClassRoomId = qrData,
					RoleId = "Student"
				};
				var newDiemRecord = new BangDiem
				{
					UserId = currentUser.Id,
					ClassRoomId = qrData,
					DTB = 0
				};
				_context.bangDiem.Add(newDiemRecord);
				_context.classroomDetail.Add(newData);
				_context.SaveChanges();

				// Trả về mã thành công 200 OK
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				// Xử lý lỗi và trả về mã lỗi 500 Internal Server Error
				return Json(new { success = false, error = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> SaveGrade(string baiNopId, decimal diem)
		{
			try
			{
				var baiNop = _context.BaiNop.FirstOrDefault(b => b.IdBaiNop.ToString() == baiNopId);
				if (baiNop == null)
				{
					return Json(new { success = false, error = "Không thể chấm điểm !!!"});
					
				}
				if (diem == baiNop.Diem)
				{
					return Json(new { success = false });
				}
				baiNop.Diem = diem;
				_context.SaveChanges();

				await TinhDTBAsync(baiNop.UserId, baiNop.ClassId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		private int DiemDD(string userId, string classId)
		{
			int diemDD; //Điểm danh = (Số buổi tham gia / Tổng số buổi học) x Điểm tối đa cho phần điểm danh.
			int tongSoBuoi = 1;
			var classRoom = _context.classRooms.FirstOrDefault(p => p.Id == classId);
			List<DiemDanh> diemDanh = _context.diemDanh.Where(p => p.UserId == userId && p.ClassRoomId == classId).ToList();
			var soBuoiHoc = diemDanh.Count() / 2;
			if (classRoom != null)
			{
				var listbuoi = CalculateClassDates(
										classRoom.StartDate,
										classRoom.EndDate,
										classRoom.DaysOfWeek,
										new TimeSpan(classRoom.StartTime.Hours, classRoom.StartTime.Minutes, 0),
										new TimeSpan(classRoom.EndTime.Hours, classRoom.EndTime.Minutes, 0)
										);
				tongSoBuoi = listbuoi.Count();
			}
			diemDD = (soBuoiHoc / tongSoBuoi) * 3;
			return diemDD;
		}

		private async Task TinhDTBAsync(string userId, string classId)
		{
			var score = await _context.bangDiem.FirstOrDefaultAsync(p => p.UserId == userId && p.ClassRoomId == classId);
			if (score != null)
			{
				int tongslBT = await _context.baiTaps.CountAsync(p => p.ClassRoomId.Trim() == classId.Trim() && p.ShowMode.Equals("All"));

				var user = await _userManager.FindByIdAsync(userId);
				string s = user.Mssv;
				int total = 0;

				string lastFour = s.Substring(s.Length - 4);
				int number = int.Parse(lastFour);
				total += number;

				if (total % 2 == 0)
				{
					int tongslBTChan = await _context.baiTaps.CountAsync(p => p.ClassRoomId.Trim() == classId && p.ShowMode.Equals("Chan"));
					tongslBT += tongslBTChan;
				}
				else
				{
					int tongslBTLe = await _context.baiTaps.CountAsync(p => p.ClassRoomId.Trim() == classId && p.ShowMode.Equals("Le"));
					tongslBT += tongslBTLe;
				}

				decimal TongDiem_User = (decimal)await _context.BaiNop
					.Where(p => p.UserId.ToString().Trim() == userId.Trim() && p.ClassId.Trim() == classId.Trim())
					.Select(p => p.Diem)
					.SumAsync();

				if (tongslBT > 0)
				{
					score.DTB = ((TongDiem_User / tongslBT) * 0.7m) + (decimal)DiemDD(userId, classId);
				}
				else
				{
					score.DTB = (decimal)DiemDD(userId, classId);
				}
			}

			await _context.SaveChangesAsync();
		}


		[HttpGet]
		public async Task<IActionResult> Search(string query)
		{
			try
			{
				var lop = userClasses.Select(p => p.Id).ToList();
				int pageSize = 5;
				IQueryable<BaiTap> queryableData = _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId));
				if (!string.IsNullOrEmpty(query))
				{
					queryableData = queryableData.Where(p => p.Title.Contains(query) || p.ClassRoomId.Contains(query));
				}

				var paginatedList = await PaginatedList<BaiTap>.CreateAsync(queryableData, 1, pageSize);

				return PartialView(paginatedList);
			}
			catch (Exception ex)
			{
				// Log the exception for troubleshooting
				Console.WriteLine(ex);
				// Optionally, return a more informative error response to the client
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetListSV(string classId)
		{

			try
			{
				if (_studentRepo.getListSV() != null)
				{
					_studentRepo.RemoveList();
				}
				var findClass = _context.classRooms.FirstOrDefault(p => p.Id == classId);
                if (findClass == null)
                {
					return Json(new { success = false, error = "Không tìm thấy lớp học !!!"});
				}
                var listUser = _context.classroomDetail.Where(p => p.ClassRoomId == classId).Select(p => p.UserId).ToList();

				var Users = await _context.Users.Where(p => listUser.Contains(p.Id)).ToListAsync();
				int stt = 1;
				foreach (var sv in Users) 
				{
					SV newsv = new SV();
					newsv.STT = stt++;
					newsv.Mssv = sv.Mssv;
					newsv.Name = sv.Name;
					newsv.Email = sv.Email;
					newsv.Userid = sv.Id;
					_studentRepo.AddSV(newsv);
				}
				List<SV> newSVs = _studentRepo.getListSV();
                return Json(new { success = true, students = Users });
            }
			catch(Exception ex)
			{
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

        [HttpPost]
        public async Task<IActionResult> ExportDSSV(string classID)
        {
            try
            {
				List<SV> students = null; 
                // Tạo một bảng tính mới
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Students");

                    // Thêm dữ liệu vào bảng tính

                    // Dòng 1 {
                    worksheet.Cells["A1:C1"].Merge = true;
                    worksheet.Cells["A1"].Value = "TRƯỜNG ĐẠI HỌC CÔNG NGHỆ TP.HCM";
                    // Truy cập vào Style của ô A1
                    var cellStyle_A1 = worksheet.Cells["A1"].Style;

                    cellStyle_A1.Font.Name = "Times New Roman"; // Tên của font chữ
                    cellStyle_A1.Font.Size = 8; // Cỡ chữ
                    cellStyle_A1.Font.Bold = true; // In đậm
                    cellStyle_A1.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    // } Hết dòng 1

                    // Dòng 3 {
                    worksheet.Cells["A3:D3"].Merge = true;
                    worksheet.Cells["A3"].Value = "DANH SÁCH SINH VIÊN";
                    // Truy cập vào Style của ô A3
                    var cellStyle_A3 = worksheet.Cells["A3"].Style;

                    cellStyle_A3.Font.Name = "Times New Roman"; // Tên của font chữ
                    cellStyle_A3.Font.Size = 16; // Cỡ chữ
                    cellStyle_A3.Font.Bold = true; // In đậm
                    cellStyle_A3.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    // } Hết dòng 3

                    // Chỉ định tên đầu mục cho mỗi cột
                    // var headerRow = worksheet.Cells[Chỉ số hàng bắt đầu (dòng 5),  Chỉ số cột bắt đầu (cột 1, tương ứng với cột A),  Chỉ số hàng kết thúc (dòng 5, vẫn là dòng 5), Chỉ số cột kết thúc (cột 4, tương ứng với cột D)];
                    List<string> headers = new List<string>
                    {
                        "STT",
                        "MSSV",
                        "Họ và tên",
                        "Email"
                    };

                    worksheet.Cells[5, 1].LoadFromArrays(new List<string[]> { headers.ToArray() });

                    var cellStyle_A5D5 = worksheet.Cells["A5:D5"].Style;

                    cellStyle_A5D5.Font.Bold = true;
                    cellStyle_A5D5.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
					students = _studentRepo.getListSV();
					worksheet.Cells[6, 1].LoadFromCollection(students, false);

                    // Tạo đối tượng Style cho border của dữ liệu được load
                    var borderStyle = worksheet.Cells[5, 1, 5 + students.Count, 4].Style.Border;

                    // Thiết lập border cho các ô
                    borderStyle.Bottom.Style = borderStyle.Top.Style = borderStyle.Left.Style = borderStyle.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    worksheet.Column(1).AutoFit();
                    worksheet.Column(2).AutoFit();
                    worksheet.Column(3).AutoFit();
                    worksheet.Column(4).AutoFit();

                    // Lấy chỉ số của hàng cuối cùng trong danh sách
                    int lastRowIndex = 5 + students.Count;

                    // Gán giá trị cho ô cuối cùng trong cột thứ 4 (cột Email)
                    worksheet.Cells[lastRowIndex + 1, 1].Value = "Tổng số sinh viên: " + students.Count;

                    // Lưu trữ bảng tính vào một luồng bộ nhớ tạm thời
                    MemoryStream stream = new MemoryStream();
                    package.SaveAs(stream);

                    _studentRepo.RemoveList();
                    // Thiết lập dữ liệu phản hồi để tải xuống tệp Excel
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DSSV_" + classID + ".xlsx");

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
		[HttpGet]
		public IActionResult DeleteLecture(int id, string classId)
		{


			var temp = _context.BaiGiang.FirstOrDefault(p => p.Id == id);
			var folderPath = Path.Combine(_environment.WebRootPath, "Uploads","BAIGIANG");
			string filePath = Path.Combine(folderPath, temp.UrlBaiGiang);
			if (temp == null)
			{
				return NotFound("Không có bài giảng này trong CSDL");
			}
			else
			{

				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
				_context.BaiGiang.Remove(temp);

			}
			_context.SaveChanges();
			return RedirectToAction("Details", "ClassRooms", new { id = classId });
		}

		[HttpGet]
		public async Task<IActionResult> DeleteBTAsync(string id, string classId)
		{
			var temp = _context.baiTaps.FirstOrDefault(p => p.Id == id);
			if (temp == null)
			{
				return NotFound("Không có bài tập này trong CSDL");
			}
			var listBN = _context.BaiNop.Where(p => p.BaiTapId == id).ToList();
			foreach (var item in listBN)
			{
				deleteFile("Uploads/BAINOP", item.Urlbainop);
				item.Diem = 0;
				_context.BaiNop.Remove(item);
			}
			if (temp.attractUrl != null)
			{
				deleteFile("Uploads/BAITAP", temp.attractUrl);
			}
			_context.baiTaps.Remove(temp);
			_context.SaveChanges();
			await TinhDTBAsync(classId);
			return RedirectToAction("Details", "ClassRooms", new { id = classId });
		}
		

		[HttpPost]
		public async Task<IActionResult> DiemDanhIn(string classId)
		{
			try
			{
				int isDate = 0;
				var currentUser = await _userManager.GetUserAsync(User);
				DateTime timeNow = DateTime.Now;
				string now = timeNow.ToString("dd/MM/yyyy");
				ClassRoom? clr = _context.classRooms.FirstOrDefault(p => p.Id == classId);
				if (clr == null)
				{
					throw new Exception("Classroom không tồn tại !!!");
				}
				
				var listdate = CalculateClassDates(
										clr.StartDate,
										clr.EndDate,
										clr.DaysOfWeek,
										new TimeSpan(clr.StartTime.Hours, clr.StartTime.Minutes, 0),
										new TimeSpan(clr.EndTime.Hours, clr.EndTime.Minutes, 0)
										);
				foreach(var date in listdate)
				{
					if (now.Equals(date.Start.ToString("dd/MM/yyyy"))){
						isDate = 1;
						break;
					}
				}
				if (isDate == 0)
				{
					throw new Exception("Hôm nay không có thời khoá biểu cho lớp này!!!");
				}
				if (timeNow.TimeOfDay.Add(TimeSpan.FromMinutes(5)) < clr.StartTime)
				{
					throw new Exception("Chưa đến giờ học !!!");
				}
				if (currentUser == null)
				{
					throw new Exception("Không có thông tin người dùng !!!");
				}
				DiemDanh? userDaDiemDanh = _context.diemDanh.FirstOrDefault(p => p.time.Trim().Substring(p.time.Length - 10, 10).Equals(now) && p.UserId == currentUser.Id && (p.Check == "IN" || p.Check == "LATE") && p.ClassRoomId == classId);
				if (userDaDiemDanh == null)
				{
                    DiemDanh dd = new DiemDanh();
                    DateTime dt = DateTime.Now;
                    dd.time = dt.ToString("hh:mm:ss - dd/MM/yyyy");
                    dd.UserId = currentUser.Id;
                    dd.ClassRoomId = classId;
                    dd.Check = timeNow.TimeOfDay.Add(TimeSpan.FromMinutes(-5)) > clr.StartTime ? "LATE" : "IN";
                    _context.diemDanh.Add(dd);
                    _context.SaveChanges();
                }
				
				return Json(new { success = true });
            }
			catch (Exception ex)
			{
                return Json(new { success = false, error = ex.Message });
            }
		}

        
        public async Task<IActionResult> DiemDanhOut(string UserID, string classId)
        {
			var currentUserID = UserID;
			DateTime totaldatenow = DateTime.Now;
            string now = totaldatenow.ToString("dd/MM/yyyy");
			var classroom = _context.classRooms.FirstOrDefault(p => p.Id == classId);
			double minTime = 0.0;
			if (classroom != null)
			{
				TimeSpan t = classroom.EndTime - classroom.StartTime;
				minTime = t.TotalMinutes * 0.7;
			}
			var userDiemDanhIn = _context.diemDanh.FirstOrDefault(p => p.time.Trim().Substring(p.time.Length - 10, 10).Equals(now) && p.UserId == currentUserID && p.Check == "IN" && p.ClassRoomId == classId);
			if (userDiemDanhIn != null)
			{
				DateTime inTime = DateTime.ParseExact(userDiemDanhIn.time.Substring(0, 8), "hh:mm:ss", null);
				double time = (totaldatenow.TimeOfDay - inTime.TimeOfDay).TotalMinutes;
				if (time >= minTime)
				{
					DiemDanh? userDaDiemDanh = _context.diemDanh.FirstOrDefault(p => p.time.Trim().Substring(p.time.Length - 10, 10).Equals(now) && p.UserId == currentUserID && p.Check == "OUT" && p.ClassRoomId == classId);
					if (userDaDiemDanh == null)
					{
						DiemDanh dd = new DiemDanh();

						dd.time = totaldatenow.ToString("hh:mm:ss - dd/MM/yyyy");
						dd.UserId = currentUserID;
						dd.ClassRoomId = classId;
						dd.Check = "OUT";
						_context.diemDanh.Add(dd);
						await _context.SaveChangesAsync();
					}
				}
			}
			await TinhDTBAsync(UserID, classId);
			return RedirectToAction("Details", "ClassRooms", new { id = classId });
        }

        [HttpPost]
		public async Task<IActionResult> changeBackground(IFormFile image, string classId)
		{
			if (image != null && !string.IsNullOrEmpty(classId))
			{
				// Xử lý lưu ảnh và liên kết với classId
				var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
				var filePath = Path.Combine(uploadsFolder, classId + "_img.png");

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await image.CopyToAsync(stream);
				}

				var classroom = _context.classRooms.FirstOrDefault(p => p.Id == classId);
				if (classroom != null)
				{
					classroom.backgroundUrl = classId + "_img.png";
				}
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Details", "ClassRooms", new { id = classId });
		}

        [HttpPost]
        public IActionResult UpdateOrder([FromBody] List<string> orderedIds)
        {
            if (orderedIds == null || !orderedIds.Any())
            {
                return BadRequest("The order list cannot be null or empty.");
            }

            foreach (var classroomId in orderedIds.Select((id, index) => new { id, index }))
            {
                var classroom = _context.classRooms.Find(classroomId.id);
                if (classroom != null)
                {
                    classroom.STT = classroomId.index+1;
                }
                else
                {
                    return BadRequest($"Classroom with ID {classroomId.id} not found.");
                }
            }

            try
            {
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

		[HttpPost]
		public async Task<IActionResult> UploadChunk(IFormFile chunk, int index, string fileName, string chunkHash)
		{
			if (chunk == null || chunk.Length == 0)
			{
				_logger.LogError("Chunk is null or empty");
				return BadRequest("Chunk is null or empty");
			}

			var uploadFolder = Path.Combine(_environment.WebRootPath,"Uploads" , "BAITAP");
			
			if (!Directory.Exists(uploadFolder))
			{
				Directory.CreateDirectory(uploadFolder);
			}
			var tempFilePath = Path.Combine(uploadFolder, fileName);
			await _semaphore.WaitAsync();
			try
			{
				// Verify chunk hash
				using (var sha256 = SHA256.Create())
				{
					using (var stream = chunk.OpenReadStream())
					{
						var computedHash = sha256.ComputeHash(stream);
						var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
						if (computedHashString != chunkHash.ToLower())
						{
							return BadRequest("Chunk hash mismatch");
						}
					}
				}

				// Append chunk to temp file
				using (var stream = new FileStream(tempFilePath, index == 0 ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.None))
				{
					await chunk.CopyToAsync(stream);
				}
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error uploading chunk");
				return StatusCode(500, "Internal server error");
			}
			finally
			{
				_semaphore.Release();
			}
		}

		[HttpPost]
		public IActionResult CompleteUpload([FromBody] CompleteUploadRequest request)
		{
			try
			{
				var uploadFolder = Path.Combine(_environment.WebRootPath, "Uploads", "BAITAP");
				var finalFilePath = Path.Combine(uploadFolder, request.FileName);

				// Compute and verify hash of the final file
				using (var sha256 = SHA256.Create())
				{
					using (var stream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read))
					{
						var finalFileHash = sha256.ComputeHash(stream);
						var finalFileHashString = BitConverter.ToString(finalFileHash).Replace("-", "").ToLower();

						// Compare with the file hash received from client
						if (finalFileHashString != request.FileHash.ToLower())
						{
							return BadRequest("Final file hash mismatch");
						}
					}
				}

				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error completing upload");
				return StatusCode(500, "Internal server error");
			}
		}

		public List<ClassDate> CalculateClassDates(DateTime startDate, DateTime endDate, string daysOfWeek, TimeSpan startTime, TimeSpan endTime)
		{
			var dates = new List<ClassDate>();
			var days = daysOfWeek.Split(',').Select(day => Enum.Parse<DayOfWeek>(day)).ToList();

			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				
				if (days.Contains(date.DayOfWeek))
				{
					var startDateTime = date.Date + startTime;
					var endDateTime = date.Date + endTime;
					dates.Add(new ClassDate(startDateTime, endDateTime, true));
				}
			}

			return dates;
		}
		
	}
}
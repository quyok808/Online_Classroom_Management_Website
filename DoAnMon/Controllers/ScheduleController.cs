using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using static DoAnMon.Models.ClassroomViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnMon.Models;

namespace DoAnMon.Controllers
{
	public class ScheduleController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public ScheduleController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public static List<ClassRoom>? userClasses;
        // GET: ScheduleController
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
                            ClassDates = CalculateClassDates(
                                        classRoom.StartDate,
                                        classRoom.EndDate,
                                        classRoom.DaysOfWeek,
                                        new TimeSpan(classRoom.StartTime.Hours, classRoom.StartTime.Minutes, 0),
                                        new TimeSpan(classRoom.EndTime.Hours, classRoom.EndTime.Minutes, 0)
                                        )
                        });
                    }
                    else
                    {
                        // Xử lý trường hợp không có chủ sở hữu (nếu cần)
                        classRoomViewModels.Add(new ClassRoomViewModel { ClassRoom = classRoom, Owner = new CustomUser { UserName = "Unknown" } });
                    }
                }
                ViewBag.diemDanh = await _context.diemDanh.Where(p => p.UserId.Equals(currentUser.Id)).ToListAsync();
            }
            // Truyền danh sách lớp học của người dùng vào View
            
            return View(classRoomViewModels);
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
                    var listDate = _context.diemDanh.Select(p => p.time.Trim().Substring(p.time.Length - 10, 10)).Distinct().ToList();
                    ClassDate newclassdate = new ClassDate(startDateTime, endDateTime);
                    foreach(var d in listDate)
                    {
                        if (date.Date.ToString("dd/MM/yyyy").Trim().Equals(d.Trim()))
                        {
                            newclassdate.AttendanceStatus = true;
                        }
                    }
                    dates.Add(newclassdate);
                }
            }

            return dates;
        }
    }
}

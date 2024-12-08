using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnMon.Data;
using DoAnMon.Models;
using DoAnMon.ViewModels;

namespace DoAnMon.Controllers
{
    public class DiemDanhsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiemDanhsController(ApplicationDbContext context)
        {
            _context = context;
        }

		// GET: DiemDanhs
		public async Task<IActionResult> Index(DateTime date, string classroomId)
        {
            DiemDanhViewModel model = new DiemDanhViewModel();
            var applicationDbContext = _context.diemDanh.Include(d => d.ClassRoom).Include(d => d.User).Where( p => p.ClassRoomId.Equals(classroomId) && p.time.Substring(p.time.IndexOf("-") + 2).Contains(date.ToString("dd/MM/yyyy")));
            model.DiemDanhs = await applicationDbContext.ToListAsync();
            model.NgayDiemDanh = date;

            return View(model);
        }

        // Your action to mark OUT
        [HttpPost]
        public IActionResult MarkOut(string userId, string classId, string date)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(classId))
            {
                return Json(new { success = false, message = "UserId and ClassId cannot be empty." });
            }
 
            try
            {
                DateTime now = DateTime.Now;
                DiemDanh newDD = new DiemDanh();
                newDD.UserId = userId;
                newDD.time = now.ToString("hh:mm:ss - dd/MM/yyyy") + " ~ " + date.Substring(date.IndexOf("-") + 2);
                newDD.ClassRoomId = classId;
                newDD.Check = "OUT";
                _context.diemDanh.Add(newDD);
                _context.SaveChanges();
                return Json(new { success = true, message = "Check out thành công !!!" });
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool DiemDanhExists(int id)
        {
            return _context.diemDanh.Any(e => e.Id == id);
        }
    }
}

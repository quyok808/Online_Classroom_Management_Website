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
using DoAnMon.Pagination;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Authorization;

namespace DoAnMon.Controllers
{
	[Authorize(Roles = "Admin, Teacher, Student")]
	public class BaiTapsController : Controller
    {
        private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;

		public BaiTapsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
			_context = context;
            _userManager = userManager;
		}

        // GET: BaiTaps

		public async Task<IActionResult> Index(string query, int pageNumber = 1)
        {
			var currentUser = await _userManager.GetUserAsync(User);
            List<string> lop = null;
			if (currentUser != null)
			{
				// Lấy danh sách lớp học mà người dùng là chủ sở hữu từ bảng classRooms
				var userClasses = await _context.classRooms.Where(p => p.UserId == currentUser.Id).ToListAsync();
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
                lop = userClasses.Select(p => p.Id).ToList();
                
			}
            int pageSize = 8;
			IQueryable<BaiTap> BaitapsQuery = _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId));
			List<BaiTap> t = _context.baiTaps.Where(p => lop.Contains(p.ClassRoomId)).ToList();
			if (!string.IsNullOrEmpty(query))
			{
				BaitapsQuery = BaitapsQuery.Where(p => p.Title.Contains(query) || p.ClassRoomId.Trim() == query.Trim());
			}
			var paginatedBaiTaps = await PaginatedList<BaiTap>.CreateAsync(BaitapsQuery, pageNumber, pageSize);
			paginatedBaiTaps.CurrentQuery = query;
			return View(paginatedBaiTaps);
		}

		// GET: BaiTaps/Details/5
		public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiTap = await _context.baiTaps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiTap == null)
            {
                return NotFound();
            }

            return View(baiTap);
        }

        // GET: BaiTaps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BaiTaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Title,attractUrl,FileFormat,HasSubmittedFile,ClassRoomId,Deadline")] BaiTap baiTap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(baiTap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(baiTap);
        }

        // GET: BaiTaps/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiTap = await _context.baiTaps.FindAsync(id);
            if (baiTap == null)
            {
                return NotFound();
            }
            return View(baiTap);
        }

        // POST: BaiTaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Content,Title,attractUrl,FileFormat,HasSubmittedFile,ClassRoomId,Deadline")] BaiTap baiTap)
        {
            if (id != baiTap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baiTap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiTapExists(baiTap.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(baiTap);
        }

        // GET: BaiTaps/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiTap = await _context.baiTaps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiTap == null)
            {
                return NotFound();
            }

            return View(baiTap);
        }

        // POST: BaiTaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var baiTap = await _context.baiTaps.FindAsync(id);
            if (baiTap != null)
            {
                _context.baiTaps.Remove(baiTap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaiTapExists(string id)
        {
            return _context.baiTaps.Any(e => e.Id == id);
        }
    }
}

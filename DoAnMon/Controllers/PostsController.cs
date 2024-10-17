using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnMon.Data;
using DoAnMon.Models;
using Microsoft.AspNetCore.Authorization;
using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Identity;
using DoAnMon.Pagination;

namespace DoAnMon.Controllers
{
	[Authorize(Roles = "Admin, Teacher, Student")]
	public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;
		public PostsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
			_userManager = userManager;
		}

		// GET: Posts
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
			int pageSize = 5;
			IQueryable<Post> PostsQuery = _context.posts.Where(p => lop.Contains(p.ClassRoomId));
			List<Post> t = _context.posts.Where(p => lop.Contains(p.ClassRoomId)).ToList();
			if (!string.IsNullOrEmpty(query))
			{
				PostsQuery = PostsQuery.Where(p => p.Title.Contains(query) || p.ClassRoomId.Trim() == query.Trim());
			}
			var paginatedPosts = await PaginatedList<Post>.CreateAsync(PostsQuery, pageNumber, pageSize);
			paginatedPosts.CurrentQuery = query;
			return View(paginatedPosts);
		}

		// GET: Posts/Details/5
		public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Title,CreateTime,ClassRoomId")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }
		[HttpPost]
		public IActionResult Editpost(string id, [Bind("Id,Content,Title,CreateTime,ClassId")] Post post)
		{
			if (ModelState.IsValid)
			{
				// Tìm bài viết trong cơ sở dữ liệu theo Id
				var existingPost = _context.posts.Find(post.Id);

				if (existingPost != null)
				{
					// Cập nhật tiêu đề và nội dung bài viết
					existingPost.Title = post.Title;
					existingPost.Content = post.Content;


					// Lưu thay đổi vào cơ sở dữ liệu
					_context.SaveChanges();

					// Chuyển hướng về trang chi tiết của lớp học
					return RedirectToAction("Details", "ClassRooms", new { id = existingPost.ClassRoomId });
				}
			}

			// Nếu dữ liệu không hợp lệ, hiển thị lại form chỉnh sửa
			return View(post);
		}

		// POST: Posts/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Content,Title,CreateTime,ClassRoomId")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "ClassRooms", new { id = post.ClassRoomId });
            }
            return View(post);
        }


		// GET: Posts/Delete/5
		public IActionResult Delete(string id, string classId)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = _context.posts.FirstOrDefault(m => m.Id == id);
			if (post == null)
			{
				return NotFound();
			}
			_context.posts.Remove(post);
			_context.SaveChanges();
			return RedirectToAction("Details", "ClassRooms", new { id = classId });
		}
		


		// POST: Posts/Delete/5
		[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var post = await _context.posts.FindAsync(id);
            if (post != null)
            {
                _context.posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


		private bool PostExists(string id)
        {
            return _context.posts.Any(e => e.Id == id);
        }


    }
}

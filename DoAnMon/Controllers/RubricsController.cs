using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using DoAnMon.Models;
using DoAnMon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace DoAnMon.Controllers
{
    public class RubricsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public RubricsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Rubrics
        [Authorize(Roles = "Admin")]
		public async Task<IActionResult> Index()
        {
            return View(await _context.Rubric.ToListAsync());
        }

        // GET: Rubrics/Details/5
        public async Task<IActionResult> Details(int? id, string classRoomId, bool? isowner)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rubric = await _context.Rubric
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rubric == null)
            {
                return NotFound();
            }
            var ListStudent = await _context.Students.Where(p => p.RubricId == id).ToListAsync();
            var classroomDetailsList = await _context.classroomDetail.Where(student => student.ClassRoomId.Equals(classRoomId)).ToListAsync();
            foreach(var student in classroomDetailsList)
            {
                var user = await _userManager.FindByIdAsync(student.UserId);
                int flag = 0;
                foreach (var item in ListStudent)
                {
                    if (item.MSSV.Equals(user.Mssv) && item.RubricId == id)
                    {
                        flag = 1; break;
                    }
                }
                if (flag == 0)
                {
                    Student newStudent = new Student();
                    newStudent.MSSV = user.Mssv;
                    newStudent.FullName = user.Name;
                    newStudent.RubricId = id;
                    _context.Students.Add(newStudent);
                    await _context.SaveChangesAsync();
                }
            }
            //var students = await _context.Students.Where(p => p.RubricId == id).ToListAsync();
            var students = await _context.Students
                             .Include(s => s.Evaluations)
                             .ThenInclude(e => e.Criterion)
                             .Where(p => p.RubricId == id)
                             .ToListAsync();
            ViewBag.Students = students;
            ViewBag.Criteria = await _context.Criteria.Where(p => p.RubricId == id).ToListAsync();
            if (isowner != null)
            {
                ViewBag.IsOwner = isowner;
            }
            return View(rubric);
        }

        // GET: Rubrics/Create
        public IActionResult Create(string classroomId)
        {
            ViewBag.ClassroomId = classroomId;
            return View();
        }

        // POST: Rubric/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRubricViewModel model, string classRoomId)
        {
            if (ModelState.IsValid)
            {
                // Tạo Rubric mới
                var rubric = new Rubric
                {
                    Name = model.Name,
                    ClassRoomId = classRoomId
				};

                // Lưu Rubric vào cơ sở dữ liệu
                _context.Rubric.Add(rubric);
                await _context.SaveChangesAsync(); // Lưu Rubric trước để có Id

                // Giải mã dữ liệu JSON thành danh sách tiêu chí
                List<Criterion> criteriaList = JsonConvert.DeserializeObject<List<Criterion>>(model.CriteriaData);

                // Lưu từng tiêu chí
                foreach (var item in criteriaList)
                {
                    Criterion newCriterion = new Criterion
                    {
                        Title = item.Title,
                        Weight = item.Weight,
                        RubricId = rubric.Id // Liên kết tiêu chí với Rubric mới tạo
                    };

                    _context.Criteria.Add(newCriterion);
                }

                // Lưu tất cả tiêu chí vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Chuyển hướng đến trang danh sách Rubric
                return RedirectToAction("Index", "ClassRooms");
            }

            return View(model); // Quay lại form nếu có lỗi
        }

        // GET: Rubrics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rubric = await _context.Rubric.FindAsync(id);
            if (rubric == null)
            {
                return NotFound();
            }
            return View(rubric);
        }

        // POST: Rubrics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Rubric rubric)
        {
            if (id != rubric.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rubric);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RubricExists(rubric.Id))
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
            return View(rubric);
        }

        // GET: Rubrics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rubric = await _context.Rubric
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rubric == null)
            {
                return NotFound();
            }

            return View(rubric);
        }

        // POST: Rubrics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rubric = await _context.Rubric.FindAsync(id);
            if (rubric != null)
            {
                var Criteria = _context.Criteria.Where(p => p.RubricId == id);
                await Criteria.ForEachAsync(p => _context.Criteria.Remove(p));
                var StudentsList = _context.Students.Where(p => p.RubricId == id);
                await StudentsList.ForEachAsync(p => _context.Students.Remove(p));
                _context.Rubric.Remove(rubric);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RubricExists(int id)
        {
            return _context.Rubric.Any(e => e.Id == id);
        }
    }
}

using DoAnMon.Data;
using DoAnMon.Models;
using DoAnMon.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnMon.Controllers
{
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _context;

		public StudentController(ApplicationDbContext context)
		{
			_context = context;
		}

        public IActionResult Create(int rubricId)
        {
            ViewBag.rubricId = rubricId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            Student newStudent = new Student();
            if (ModelState.IsValid)
            {
                newStudent.MSSV = student.MSSV;
                newStudent.FullName = student.FullName;
                newStudent.RubricId = student.RubricId;
                _context.Students.Add(newStudent);
                _context.SaveChanges();
                return RedirectToAction("Details", "Rubrics", new { id = newStudent.RubricId });
            }
            return View(newStudent);
        }

        public IActionResult Evaluate(int id, int RubricId)
        {
            var student = _context.Students
                                  .Include(s => s.Evaluations)
                                  .ThenInclude(e => e.Criterion)
                                  .FirstOrDefault(s => s.Id == id);
            ViewBag.Criteria = _context.Criteria.Where(p => p.RubricId == RubricId).ToList();
            return View(student);
        }

        [HttpPost]
        public IActionResult Evaluate(int id, List<Evaluation> evaluations, int RubricId)
        {
            int? rubricId = RubricId;
            var student = _context.Students
                                  .Include(s => s.Evaluations)
                                  .FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            foreach (var eval in evaluations)
            {
                var existingEvaluation = student.Evaluations
                                                .FirstOrDefault(e => e.CriterionId == eval.CriterionId);

                if (existingEvaluation != null)
                {
                    existingEvaluation.Score = eval.Score; // Cập nhật điểm nếu đánh giá đã tồn tại
                }
                else
                {
                    // Thêm mới đánh giá nếu chưa có
                    student.Evaluations.Add(new Evaluation
                    {
                        StudentId = id,
                        CriterionId = eval.CriterionId,
                        Score = eval.Score
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Details", "Rubrics", new { id = RubricId });
        }

        [HttpPost]
        public IActionResult UpdateScore([FromBody] ScoreUpdateRequest request)
        {
            // Tìm sinh viên và tiêu chí cần cập nhật
            var student = _context.Students
                .Include(s => s.Evaluations)
                .ThenInclude(e => e.Criterion)
                .FirstOrDefault(s => s.Id == request.StudentId);

            if (student != null)
            {
                var evaluation = student.Evaluations
                    .FirstOrDefault(e => e.CriterionId == request.CriterionId);

                if (evaluation == null)
                {
                    Evaluation newEvaluation = new Evaluation();
                    newEvaluation.Score = request.Score;
                    newEvaluation.StudentId = request.StudentId;
                    newEvaluation.CriterionId = request.CriterionId;
                    _context.Evaluations.Add(newEvaluation);
                }
                else
                {
                    evaluation.Score = request.Score;
                }
                _context.SaveChanges();

                // Tính lại điểm tổng
                // Tải lại dữ liệu student với các Evaluation và Criterion để đảm bảo không bị null
                student = _context.Students
                    .Include(s => s.Evaluations)
                    .ThenInclude(e => e.Criterion)
                    .FirstOrDefault(s => s.Id == request.StudentId);

                if (student != null)
                {
                    var newTotalScore = student.Evaluations.Sum(e => e.Score * (e.Criterion?.Weight ?? 0));
                    return Json(new { success = true, newTotalScore });
                }
            }

            return Json(new { success = false });
        }
    }
}

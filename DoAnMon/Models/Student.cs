using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string MSSV { get; set; }
        public string FullName { get; set; }
        public int? RubricId { get; set; }
        [ValidateNever]
        public Rubric? Rubric { get; set; }
        public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class Criterion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public float Weight { get; set; }
        public int? RubricId { get; set; }
        public Rubric? Rubric { get; set; }
        [ValidateNever]
        public List<Evaluation> Evaluations { get; set; }
    }
}

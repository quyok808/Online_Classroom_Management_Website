namespace DoAnMon.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int CriterionId { get; set; }
        public Criterion Criterion { get; set; }
        public float Score { get; set; }
    }
}

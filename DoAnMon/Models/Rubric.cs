using DoAnMon.ModelListSVDownload;

namespace DoAnMon.Models
{
    public class Rubric
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
        public List<Student>? Student { get; set; }
        public List<Criterion>? Criteria { get; set; }
    }
}

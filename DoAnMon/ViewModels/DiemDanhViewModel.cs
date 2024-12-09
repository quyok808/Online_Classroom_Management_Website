using DoAnMon.Models;

namespace DoAnMon.ViewModels
{
    public class DiemDanhViewModel
    {
        public List<DiemDanh>? DiemDanhs { get; set; }
        public DateTime NgayDiemDanh { get; set; }
        public string OwnerId;
    }
}

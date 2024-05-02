using Microsoft.EntityFrameworkCore.Metadata;

namespace DoAnMon.Models
{
	public class DiemViewModel
	{
		public string HoVaTen {  get; set; }
		public string MSSV {  get; set; }
		public List<decimal> listDiemBT { get; set; }
		public decimal DTB { get; set; }
	}
}

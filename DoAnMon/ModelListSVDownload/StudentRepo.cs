
namespace DoAnMon.ModelListSVDownload
{
	public class StudentRepo : IStudent
	{
		static List<SV> listsv;

		public StudentRepo()
		{
			if (listsv == null)
			{
				listsv = new List<SV>();
			}
		}

		public void AddSV(SV sv)
		{
			listsv.Add(sv);

		}

		public List<SV> getListSV()
		{
			return listsv;
		}

		public void RemoveList()
		{
			listsv.Clear();
		}
	}
}

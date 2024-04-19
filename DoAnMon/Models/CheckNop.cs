using DoAnMon.Data;

namespace DoAnMon.Models
{
	public class CheckNop : ICheckNop
	{
		private readonly ApplicationDbContext _context; // Thay YourDbContext bằng DbContext của bạn

		public CheckNop(ApplicationDbContext context)
		{
			_context = context;
		}

		public bool HasUserSubmittedBaiTap(string userId, string baiTapId)
		{
			var hasSubmitted = _context.BaiNop.Any(bn => bn.BaiTapId == baiTapId && bn.UserId == userId);
			return hasSubmitted;
		}
	}
}

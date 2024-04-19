namespace DoAnMon.Models
{
	public interface ICheckNop
	{
		bool HasUserSubmittedBaiTap(string userId, string baiTapId);
	}
}

namespace DoAnMon.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string RequesterId { get; set; }
        public string TargetId { get; set; }
        public DateTime? createAt { get; set; }
        public string? ClassID { get; set; }
        public bool IsAccepted { get; set; }
    }
}

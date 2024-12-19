using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class LeaveRequest
    {
        public int Id {  get; set; }
        public string UserID { get; set; }
        public string ClassRoomId { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
        public string? Reasion {  get; set; }
        public DateTime ThoiGianYeuCau {  get; set; }
        public int Status { get; set; }
        public string? Image {  get; set; }

        [ValidateNever]
        public ClassRoom? ClassRoom { get; set; }
        [ValidateNever]
        public CustomUser? User { get; set; }
    }
}

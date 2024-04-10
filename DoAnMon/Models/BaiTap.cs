using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class BaiTap
    {
        public string Id { get; set; }
        public string? Content { get; set; }
        [ValidateNever]
        public ICollection<BaiTapDetail> BaiTapDetails { get; set; }
    }
}

namespace DoAnMon.Models
{
    public class BaiGiang
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UrlBaiGiang { get; set; }

        public string ClassId { get; set; }

        public ClassRoom ClassRoom { get; set; }
    }
}

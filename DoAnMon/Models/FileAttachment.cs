namespace DoAnMon.Models
{
	public class FileAttachment
	{
		public int Id { get; set; }
		public string FileName { get; set; }  
		public string FilePath { get; set; }  
		public string FileType { get; set; }  
		public int MessageId { get; set; }    
		public Message? Message { get; set; }
	}
}

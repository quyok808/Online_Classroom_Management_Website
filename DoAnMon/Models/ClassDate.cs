namespace DoAnMon.Models
{
	public class ClassDate
	{
        public ClassDate(DateTime start, DateTime end, bool attendanceStatus)
        {
            Start = start;
            End = end;
            AttendanceStatus = attendanceStatus;
        }
        public ClassDate(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            AttendanceStatus = false;
        }
        public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public bool AttendanceStatus { get; set; } // True if attended, False if not
	}

}

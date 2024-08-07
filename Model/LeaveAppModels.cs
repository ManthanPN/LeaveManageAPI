namespace LeaveManageAPI.Model
{
    public class LeaveApplication
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Reason { get; set; }
        public string? Status { get; set; }
        public string TypeLeave { get; set; }
        public string LeaveDuration { get; set; }
    }
}

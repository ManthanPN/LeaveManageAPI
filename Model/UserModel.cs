namespace LeaveManageAPI.Model
{
    public class RegsiterModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int LeaveDays { get; set; } = 26; 
    }

    public class LoginModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

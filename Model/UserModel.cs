namespace LeaveManageAPI.Model
{
    public class RegsiterModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int LeaveDays { get; set; } = 26;
        public string? Email { get; set; }
        public string? Birthdate { get; set; }
    }

    public class LoginModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UpdateUserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Birthdate { get; set; }
    }
}

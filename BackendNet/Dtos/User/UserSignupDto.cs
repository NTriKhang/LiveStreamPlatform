namespace BackendNet.Dtos.User
{
    public class UserSignupDto
    {
        public string UserName { get; set; }
        public string Email { get; set; } = "";
        public string DislayName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }
}

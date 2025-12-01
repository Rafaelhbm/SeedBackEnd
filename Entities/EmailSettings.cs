namespace SeedBackend_V1.Entities
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string AdminEmail { get; set; } = null!;
    }
}

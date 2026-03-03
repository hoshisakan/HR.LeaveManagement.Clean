namespace HR.LeaveManagement.Application.Models.Identity
{
    public class RefreshTokenDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

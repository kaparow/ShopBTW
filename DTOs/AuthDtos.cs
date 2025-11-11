namespace ShopBTW.DTOs
{
        public record RegisterDto(string FirstName, string LastName, string Email, string Password);
        public record LoginDto(string Email, string Password);
        public record AuthResultDto(int CustomerId, string Email, string Token, DateTime ExpiresAtUtc);
}

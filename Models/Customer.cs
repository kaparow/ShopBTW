namespace ShopBTW.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Навигация
        public List<Order> Orders { get; set; } = [];

        // ===== БИЗНЕС-ЛОГИКА =====

        public string GetFullName() => $"{FirstName} {LastName}".Trim();

        // Простейшая проверка
        public bool HasEmail() => !string.IsNullOrWhiteSpace(Email);
    }
}

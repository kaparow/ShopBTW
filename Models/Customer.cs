namespace ShopBTW.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = string.Empty; // добавили
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

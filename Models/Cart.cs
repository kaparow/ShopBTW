// ShopBTW/Models/Cart.cs
namespace ShopBTW.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public bool IsCheckedOut { get; set; }   // false — активная корзина
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

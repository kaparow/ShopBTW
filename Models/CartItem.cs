namespace ShopBTW.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // 👈 добавь это
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}

namespace ShopBTW.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Внешний ключ на Customer
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Позиции заказа
        public List<OrderItem> Items { get; set; } = [];

        // ===== БИЗНЕС-ЛОГИКА =====

        // Добавить позицию
        public void AddItem(int productId, string productName, decimal price, int quantity)
        {
            if (quantity <= 0) return;

            Items.Add(new OrderItem
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity
            });
        }

        // Посчитать сумму
        public decimal GetTotal() => Items.Sum(i => i.Price * i.Quantity);

        // Сколько разных товаров
        public int GetDistinctProductsCount() => Items.Select(i => i.ProductId).Distinct().Count();
    }

    // Вспомогательный класс – можно тоже вынести в Models
    public class OrderItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        // FK на Order
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}

namespace ShopBTW.Models
{
    public class Product
    {
        public int Id { get; set; }

        // Название товара
        public string Name { get; set; } = string.Empty;

        // Базовая цена
        public decimal Price { get; set; }

        // Остаток на складе
        public int Stock { get; set; }

        // ===== БИЗНЕС-ЛОГИКА =====

        // Списать со склада
        public bool Reserve(int quantity)
        {
            if (quantity <= 0) return false;
            if (Stock < quantity) return false;

            Stock -= quantity;
            return true;
        }

        // Применить скидку в %
        public void ApplyDiscount(decimal percent)
        {
            if (percent <= 0) return;
            if (percent > 90) percent = 90;
            Price = Price - Price * (percent / 100m);
        }
    }
}

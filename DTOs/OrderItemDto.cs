namespace ShopBTW.DTOs;

// Это DTO для ответа и деталей заказа (не для входа!)
public record OrderItemDto(int ProductId, string ProductName, decimal Price, int Quantity);

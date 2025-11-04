namespace ShopBTW.DTOs;

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    decimal Price,
    int Quantity
);

namespace ShopBTW.DTOs;

public record OrderDto(
    int Id,
    string CustomerName,
    DateTime CreatedAt,
    IEnumerable<OrderItemDto> Items,
    decimal Total
);

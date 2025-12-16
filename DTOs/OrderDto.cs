namespace ShopBTW.DTOs;

public record OrderDto(int Id, DateTime CreatedAt, decimal Total, IEnumerable<OrderItemDto> Items);

namespace ShopBTW.DTOs;

public record CreateOrderDto(IEnumerable<CreateOrderItemDto> Items);

public record CreateOrderItemDto(int ProductId, int Quantity);

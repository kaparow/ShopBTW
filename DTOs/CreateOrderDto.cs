namespace ShopBTW.DTOs;

public record CreateOrderDto(
    int CustomerId,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    int ProductId,
    string ProductName,   // если оставляешь текущую модель без «догрузки»
    decimal Price,
    int Quantity
);

namespace ShopBTW.DTOs;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    int Stock
);

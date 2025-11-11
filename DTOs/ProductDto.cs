using ShopBTW.Models;

namespace ShopBTW.DTOs;

public record ProductDto(
    int Id,
    string Name,
    RobotPartType PartType,
    decimal Price,
    int Stock
);

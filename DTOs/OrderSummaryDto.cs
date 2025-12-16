namespace ShopBTW.DTOs;

public record OrderSummaryDto(
    int Id,
    DateTime CreatedAt,
    int ItemsCount,
    decimal Total
);

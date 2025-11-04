namespace ShopBTW.DTOs;

public record CustomerDto(
    int Id,
    string FullName,
    string Email,
    IEnumerable<OrderSummaryDto> Orders
);

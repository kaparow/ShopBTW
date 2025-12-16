// ShopBTW/DTOs/CartDtos.cs
namespace ShopBTW.DTOs
{
    public record CartItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
    public record CartDto(int Id, int CustomerId, decimal Total, IEnumerable<CartItemDto> Items);

    public record AddToCartDto(int ProductId, int Quantity);
    public record UpdateQuantityDto(int Quantity);

    public record CheckoutResultDto(int OrderId, DateTime CreatedAt, decimal Total);
}

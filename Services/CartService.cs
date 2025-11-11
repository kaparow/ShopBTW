using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;

namespace ShopBTW.Services;

public class CartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    // Получить корзину клиента
    public async Task<CartDto> GetCartAsync(int customerId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return new CartDto(
            cart.Id,
            cart.CustomerId,
            cart.IsCheckedOut,
            cart.Items.Sum(i => i.Quantity * i.UnitPrice),
            cart.Items.Select(i => new CartItemDto(
                i.ProductId,
                i.Product.Name,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
        );
    }

    // Добавить товар
    public async Task<CartDto> AddItemAsync(int customerId, int productId, int quantity)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId };
            _db.Carts.Add(cart);
        }

        var product = await _db.Products.FindAsync(productId)
            ?? throw new Exception("Product not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity += quantity;
        }

        await _db.SaveChangesAsync();
        return await GetCartAsync(customerId);
    }

    // Оформить заказ
    public async Task<CheckoutResultDto> CheckoutAsync(int customerId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut)
            ?? throw new Exception("Cart not found");

        var order = new Order
        {
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        cart.IsCheckedOut = true;

        await _db.SaveChangesAsync();

        return new CheckoutResultDto(order.Id, order.CreatedAt, order.Items.Sum(i => i.Price * i.Quantity));
    }
}

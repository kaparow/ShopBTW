using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;

namespace ShopBTW.Services;

public class CartService
{
    private readonly AppDbContext _db;
    public CartService(AppDbContext db) => _db = db;

    // Текущая корзина пользователя
    public async Task<CartDto> GetCartAsync(int customerId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)                 // Product не нужен (берём снапшот имени)
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
            cart.Items.Sum(i => i.Quantity * i.UnitPrice),
            cart.Items.Select(i => new CartItemDto(
                i.ProductId,
                i.ProductName,                     // <-- берём имя из CartItem
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
        );
    }

    // Добавить товар в корзину
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
                ProductId = product.Id,
                ProductName = product.Name,        // <-- СНАПШОТ имени
                UnitPrice = product.Price,         // <-- СНАПШОТ цены
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

    // Изменить количество (на случай, если у тебя есть PATCH)
    public async Task<CartDto> UpdateQuantityAsync(int customerId, int productId, int quantity)
    {
        var cart = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut)
            ?? throw new Exception("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new Exception("Item not found");

        if (quantity <= 0) cart.Items.Remove(item);
        else item.Quantity = quantity;

        await _db.SaveChangesAsync();
        return await GetCartAsync(customerId);
    }

    // Удалить позицию (если нужно)
    public async Task<CartDto> RemoveItemAsync(int customerId, int productId)
    {
        var cart = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut)
            ?? throw new Exception("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) _db.CartItems.Remove(item);

        await _db.SaveChangesAsync();
        return await GetCartAsync(customerId);
    }

    // Оформить заказ
    public async Task<CheckoutResultDto> CheckoutAsync(int customerId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut)
            ?? throw new Exception("Cart not found");

        if (cart.Items.Count == 0)
            throw new Exception("Cart is empty");

        var order = new Order
        {
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,       // <-- переносим имя из корзины
                Price = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        cart.IsCheckedOut = true;

        await _db.SaveChangesAsync();

        var total = order.Items.Sum(i => i.Price * i.Quantity);
        return new CheckoutResultDto(order.Id, order.CreatedAt, total);
    }
}


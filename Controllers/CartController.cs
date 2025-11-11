using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;
using System.Security.Claims;

namespace ShopBTW.Controllers;

[Authorize] // доступ только с JWT
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    // Берём Id текущего пользователя из токена
    private int GetCustomerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Преобразование сущности в DTO
    private static CartDto ToDto(Cart c) =>
        new CartDto(
            c.Id,
            c.CustomerId,
            c.IsCheckedOut,
            c.Items.Sum(i => i.UnitPrice * i.Quantity),
            c.Items.Select(i => new CartItemDto(
                i.ProductId,
                i.Product.Name,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
        );

    // Получить/создать активную корзину пользователя
    private async Task<Cart> GetOrCreateActiveCart(int customerId, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut, ct);

        if (cart is null)
        {
            cart = new Cart { CustomerId = customerId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
            await _db.Entry(cart).Collection(c => c.Items).LoadAsync(ct);
        }
        return cart;
    }

    // GET: api/cart — показать корзину
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
    {
        var customerId = GetCustomerId();
        var cart = await GetOrCreateActiveCart(customerId, ct);
        return ToDto(cart);
    }

    // POST: api/cart/items — добавить позицию
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartDto dto, CancellationToken ct)
    {
        if (dto.Quantity <= 0) return BadRequest("Quantity must be > 0.");

        var customerId = GetCustomerId();
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProductId, ct);
        if (product is null) return NotFound("Product not found.");

        var cart = await GetOrCreateActiveCart(customerId, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (item is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = dto.Quantity,
                UnitPrice = product.Price // фиксируем цену при добавлении
            });
        }
        else
        {
            item.Quantity += dto.Quantity;
        }

        await _db.SaveChangesAsync(ct);
        await _db.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync(ct);
        return ToDto(cart);
    }

    // PATCH: api/cart/items/{productId} — изменить количество (0 или меньше = удалить)
    [HttpPatch("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> UpdateQuantity(int productId, [FromBody] UpdateQuantityDto dto, CancellationToken ct)
    {
        var customerId = GetCustomerId();
        var cart = await GetOrCreateActiveCart(customerId, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return NotFound("Item not found in cart.");

        if (dto.Quantity <= 0)
            _db.CartItems.Remove(item);
        else
            item.Quantity = dto.Quantity;

        await _db.SaveChangesAsync(ct);
        await _db.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync(ct);
        return ToDto(cart);
    }

    // DELETE: api/cart/items/{productId} — удалить позицию
    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int productId, CancellationToken ct)
    {
        var customerId = GetCustomerId();
        var cart = await GetOrCreateActiveCart(customerId, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return NotFound("Item not found.");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync(ct);
        return ToDto(cart);
    }

    // POST: api/cart/checkout — оформить заказ
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(CancellationToken ct)
    {
        var customerId = GetCustomerId();

        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut, ct);

        if (cart is null || !cart.Items.Any())
            return BadRequest("Cart is empty.");

        var order = new Order
        {
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                Price = i.UnitPrice // берём зафиксированную цену
            }).ToList()
        };

        _db.Orders.Add(order);
        cart.IsCheckedOut = true;

        await _db.SaveChangesAsync(ct);

        var total = order.Items.Sum(i => i.Price * i.Quantity);
        return new CheckoutResultDto(order.Id, order.CreatedAt, total);
    }

    // (опционально) DELETE: api/cart — очистить корзину
    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var customerId = GetCustomerId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut, ct);

        if (cart is null) return NoContent();

        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

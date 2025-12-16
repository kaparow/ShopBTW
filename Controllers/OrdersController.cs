using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;
using System.Security.Claims;

namespace ShopBTW.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    private int CurrentCustomerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: api/orders — только заказы текущего пользователя
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(CancellationToken ct)
    {
        var customerId = CurrentCustomerId();

        var data = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderDto(
                o.Id,
                o.CreatedAt,
                o.Items.Sum(i => i.Price * i.Quantity),
                o.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,    // <-- берём из снимка названия в OrderItem
                    i.Price,
                    i.Quantity
                ))
            ))
            .ToListAsync(ct);

        return Ok(data);
    }

    // GET: api/orders/{id} — один заказ текущего пользователя
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> Get(int id, CancellationToken ct)
    {
        var customerId = CurrentCustomerId();

        var dto = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && o.CustomerId == customerId)
            .Select(o => new OrderDto(
                o.Id,
                o.CreatedAt,
                o.Items.Sum(i => i.Price * i.Quantity),
                o.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,    // <-- только поле снимка
                    i.Price,
                    i.Quantity
                ))
            ))
            .FirstOrDefaultAsync(ct);

        return dto is null ? NotFound() : Ok(dto);
    }

    // POST: api/orders — name/price подтягиваются из БД, клиент шлёт только productId/quantity
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        if (dto.Items is null || !dto.Items.Any())
            return BadRequest("Order must contain at least one item.");

        var customerId = CurrentCustomerId();

        // Получаем все нужные продукты одним запросом
        var ids = dto.Items.Select(i => i.ProductId).Distinct().ToArray();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // Проверяем валидность productId
        foreach (var it in dto.Items)
            if (!products.ContainsKey(it.ProductId))
                return BadRequest($"Product {it.ProductId} not found.");

        // Формируем заказ: сохраняем снимок имени и цены
        var order = new Order
        {
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(it =>
            {
                var p = products[it.ProductId];
                return new OrderItem
                {
                    ProductId = p.Id,
                    ProductName = p.Name,   // <-- СНИМОК названия
                    Price = p.Price,        // <-- СНИМОК цены
                    Quantity = it.Quantity
                };
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        var result = new OrderDto(
            order.Id,
            order.CreatedAt,
            order.Items.Sum(i => i.Price * i.Quantity),
            order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Price,
                i.Quantity
            ))
        );

        return CreatedAtAction(nameof(Get), new { id = order.Id }, result);
    }

    // DELETE: api/orders/{id} — удаляет только свой заказ
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var customerId = CurrentCustomerId();

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, ct);

        if (order is null) return NotFound();

        _db.OrderItems.RemoveRange(order.Items);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

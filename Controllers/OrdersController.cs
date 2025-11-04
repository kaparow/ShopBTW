using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(CancellationToken ct)
    {
        var data = await db.Orders
            .AsNoTracking()
            .Select(o => new OrderDto(
                o.Id,
                o.Customer != null ? (o.Customer.FirstName + " " + o.Customer.LastName) : "",
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity
                )),
                o.Items.Sum(i => i.Price * i.Quantity)
            ))
            .ToListAsync(ct);

        return data;
    }

    // GET: api/orders/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> Get(int id, CancellationToken ct)
    {
        var dto = await db.Orders
            .Where(o => o.Id == id)
            .AsNoTracking()
            .Select(o => new OrderDto(
                o.Id,
                o.Customer != null ? (o.Customer.FirstName + " " + o.Customer.LastName) : "",
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity
                )),
                o.Items.Sum(i => i.Price * i.Quantity)
            ))
            .FirstOrDefaultAsync(ct);

        return dto is null ? NotFound() : dto;
    }

    // POST: api/orders  (вариант без «догрузки» по ProductId — как ты просил)
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto, CancellationToken ct)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return BadRequest("Order must contain at least one item");

        var exists = await db.Customers.AnyAsync(c => c.Id == dto.CustomerId, ct);
        if (!exists) return BadRequest($"Customer {dto.CustomerId} not found");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(it => new OrderItem
            {
                ProductId = it.ProductId,
                ProductName = it.ProductName,
                Price = it.Price,
                Quantity = it.Quantity
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var result = new OrderDto(
            order.Id,
            await db.Customers.Where(c => c.Id == order.CustomerId)
                              .Select(c => c.FirstName + " " + c.LastName)
                              .FirstOrDefaultAsync(ct) ?? "",
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity
            )),
            order.Items.Sum(i => i.Price * i.Quantity)
        );

        return CreatedAtAction(nameof(Get), new { id = order.Id }, result);
    }

    // DELETE: api/orders/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var model = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (model is null) return NotFound();

        db.OrderItems.RemoveRange(model.Items);
        db.Orders.Remove(model);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

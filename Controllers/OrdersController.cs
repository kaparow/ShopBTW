using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.Models;
using ShopBTW.Data;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken ct)
        => await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .AsNoTracking()
            .ToListAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> Get(int id, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return order is null ? NotFound() : order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order model, CancellationToken ct)
    {
        model.Items ??= new List<OrderItem>();
        db.Orders.Add(model);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    // DTO для добавления позиции
    public record AddItemDto(int ProductId, string ProductName, decimal Price, int Quantity);

    // POST: api/orders/5/add-item
    [HttpPost("{id:int}/add-item")]
    public async Task<IActionResult> AddItem(int id, AddItemDto dto, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return NotFound();

        order.AddItem(dto.ProductId, dto.ProductName, dto.Price, dto.Quantity);
        await db.SaveChangesAsync(ct);
        return Ok(order);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await db.Orders.FindAsync([id], ct);
        if (entity is null) return NotFound();
        db.Orders.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

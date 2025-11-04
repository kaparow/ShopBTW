using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    // 1) Заказы с ФИО клиента и суммой (Orders + Customers + Items)
    // GET: api/reports/orders-with-customers
    [HttpGet("orders-with-customers")]
    public async Task<IActionResult> OrdersWithCustomers(CancellationToken ct)
    {
        var data = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Select(o => new
            {
                o.Id,
                Customer = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "—",
                CreatedAt = o.CreatedAt,
                ItemsCount = o.Items.Count,
                Total = o.Items.Sum(i => i.Price * i.Quantity)
            })
            .ToListAsync(ct);

        return Ok(data);
    }

    // 2) Заказы конкретного клиента (Where + Select)
    // GET: api/reports/customer-orders/1
    [HttpGet("customer-orders/{customerId:int}")]
    public async Task<IActionResult> CustomerOrders(int customerId, CancellationToken ct)
    {
        var data = await db.Orders
            .Where(o => o.CustomerId == customerId)
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                Total = o.Items.Sum(i => i.Price * i.Quantity)
            })
            .ToListAsync(ct);

        return Ok(data);
    }

    // 3) Товары с малым остатком (Where + Select)
    // GET: api/reports/low-stock?limit=5
    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var data = await db.Products
            .Where(p => p.Stock < limit)
            .Select(p => new { p.Id, p.Name, p.Stock })
            .ToListAsync(ct);

        return Ok(data);
    }

    // 4) ТОП товаров по количеству в заказах (GroupBy)
    // GET: api/reports/top-products
    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts(CancellationToken ct)
    {
        var data = await db.OrderItems
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                TotalQuantity = g.Sum(x => x.Quantity),
                OrdersCount = g.Count()
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ToListAsync(ct);

        return Ok(data);
    }
}

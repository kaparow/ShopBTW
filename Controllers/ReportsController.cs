using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    // 1) Заказы с именем клиента и суммой
    // GET: api/reports/orders-with-customers
    [HttpGet("orders-with-customers")]
    public async Task<IActionResult> OrdersWithCustomers(CancellationToken ct)
    {
        var data = await db.Orders
            .AsNoTracking()
            .Select(o => new
            {
                o.Id,
                Customer = o.Customer != null ? (o.Customer.FirstName + " " + o.Customer.LastName) : "",
                o.CreatedAt,
                Total = o.Items.Sum(i => i.Price * i.Quantity)
            })
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(data);
    }

    // 2) Топ-товары по количеству (сумма quantity по всем заказам)
    // GET: api/reports/top-products
    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts(CancellationToken ct)
    {
        var data = await db.OrderItems
            .AsNoTracking()
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Price * x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ToListAsync(ct);

        return Ok(data);
    }

    // 3) Сводка по клиентам: кол-во заказов и общая сумма
    // GET: api/reports/customer-summary
    [HttpGet("customer-summary")]
    public async Task<IActionResult> CustomerSummary(CancellationToken ct)
    {
        var data = await db.Customers
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                Name = c.FirstName + " " + c.LastName,
                OrdersCount = c.Orders.Count,
                Total = c.Orders.SelectMany(o => o.Items).Sum(i => i.Price * i.Quantity)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync(ct);

        return Ok(data);
    }
}

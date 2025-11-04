using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(AppDbContext db) : ControllerBase
{
    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(CancellationToken ct)
    {
        var data = await db.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto(
                c.Id,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.Orders.Select(o => new OrderSummaryDto(
                    o.Id,
                    o.CreatedAt,
                    o.Items.Count,
                    o.Items.Sum(i => i.Price * i.Quantity)
                ))
            ))
            .ToListAsync(ct);

        return data;
    }

    // GET: api/customers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Get(int id, CancellationToken ct)
    {
        var dto = await db.Customers
            .Where(c => c.Id == id)
            .AsNoTracking()
            .Select(c => new CustomerDto(
                c.Id,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.Orders.Select(o => new OrderSummaryDto(
                    o.Id,
                    o.CreatedAt,
                    o.Items.Count,
                    o.Items.Sum(i => i.Price * i.Quantity)
                ))
            ))
            .FirstOrDefaultAsync(ct);

        return dto is null ? NotFound() : dto;
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        CustomerCreateDto dtoIn, CancellationToken ct)
    {
        var model = new Customer
        {
            FirstName = dtoIn.FirstName,
            LastName = dtoIn.LastName,
            Email = dtoIn.Email
        };

        db.Customers.Add(model);
        await db.SaveChangesAsync(ct);

        var dtoOut = new CustomerDto(
            model.Id,
            model.FirstName + " " + model.LastName,
            model.Email,
            Enumerable.Empty<OrderSummaryDto>());

        return CreatedAtAction(nameof(Get), new { id = model.Id }, dtoOut);
    }

    // PUT: api/customers/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomerCreateDto dto, CancellationToken ct)
    {
        var model = await db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (model is null) return NotFound();

        model.FirstName = dto.FirstName;
        model.LastName = dto.LastName;
        model.Email = dto.Email;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE: api/customers/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var model = await db.Customers.FindAsync([id], ct);
        if (model is null) return NotFound();

        db.Customers.Remove(model);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

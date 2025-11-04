using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.Models;
using ShopBTW.Data;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll(CancellationToken ct)
    {
        var customers = await db.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Items)
            .AsNoTracking()
            .ToListAsync(ct);

        return customers;
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> Get(int id, CancellationToken ct)
    {
        var customer = await db.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return customer is null ? NotFound() : customer;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer model, CancellationToken ct)
    {
        db.Customers.Add(model);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }   

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Customer model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest("Id mismatch");
        var exists = await db.Customers.AnyAsync(c => c.Id == id, ct);
        if (!exists) return NotFound();

        db.Entry(model).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await db.Customers.FindAsync([id], ct);
        if (entity is null) return NotFound();
        db.Customers.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

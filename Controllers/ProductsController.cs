using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.Models;
using ShopBTW.Data;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken ct)
        => await db.Products.AsNoTracking().ToListAsync(ct);

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Get(int id, CancellationToken ct)
    {
        var entity = await db.Products.FindAsync([id], ct);
        return entity is null ? NotFound() : entity;
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product model, CancellationToken ct)
    {
        db.Products.Add(model);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest("Id mismatch");
        var exists = await db.Products.AnyAsync(p => p.Id == id, ct);
        if (!exists) return NotFound();

        db.Entry(model).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await db.Products.FindAsync([id], ct);
        if (entity is null) return NotFound();
        db.Products.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // Бизнес-логика: применить скидку
    // POST: api/products/5/discount/10
    [HttpPost("{id:int}/discount/{percent:decimal}")]
    public async Task<IActionResult> ApplyDiscount(int id, decimal percent, CancellationToken ct)
    {
        var prod = await db.Products.FindAsync([id], ct);
        if (prod is null) return NotFound();
        prod.ApplyDiscount(percent);
        await db.SaveChangesAsync(ct);
        return Ok(prod);
    }
}

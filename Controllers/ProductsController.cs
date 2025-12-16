using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBTW.Data;
using ShopBTW.DTOs;
using ShopBTW.Models;

namespace ShopBTW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken ct)
        => await db.Products.AsNoTracking()
            .Select(p => new ProductDto(
                p.Id, p.Name, p.PartType, p.Price, p.Stock))
            .ToListAsync(ct);

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> Get(int id, CancellationToken ct)
    {
        var dto = await db.Products.Where(p => p.Id == id)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.PartType, p.Price, p.Stock))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return dto is null ? NotFound() : dto;
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(Product model, CancellationToken ct)
    {
        if (!IsAllowedPart(model.PartType))
            return BadRequest("Only robot parts are allowed (Tracks, Motor).");

        db.Products.Add(model);
        await db.SaveChangesAsync(ct);

        var dto = new ProductDto(
            model.Id, model.Name, model.PartType, model.Price, model.Stock);

        return CreatedAtAction(nameof(Get), new { id = model.Id }, dto);
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest("Id mismatch");
        if (!IsAllowedPart(model.PartType))
            return BadRequest("Only robot parts are allowed (Tracks, Motor).");

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

    private static bool IsAllowedPart(RobotPartType t)
        => t is RobotPartType.Tracks or RobotPartType.Motor or RobotPartType.Battery or RobotPartType.Wheel or RobotPartType.Controller;
}

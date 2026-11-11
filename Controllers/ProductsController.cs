using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;
using MyPharmacy.Security;

namespace MyPharmacy.Controllers;

[Route("Products")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ProductsController(ApplicationDbContext db) => _db = db;

    [HasPermission("products.view")]
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Products.OrderBy(p => p.Name).ToListAsync());

    [HasPermission("products.create")]
    [HttpGet("Create")]
    public IActionResult Create() => View(new Product());

    [HasPermission("products.create")]
    [HttpPost("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Products.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("products.edit")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Products.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HasPermission("products.edit")]
    [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Products.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("products.delete")]
    [HttpPost("Delete/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Products.FindAsync(id);
        if (item != null) { _db.Products.Remove(item); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}

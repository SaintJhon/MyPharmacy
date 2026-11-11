using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;
using MyPharmacy.Security;

namespace MyPharmacy.Controllers;

[Route("Suppliers")]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _db;
    public SuppliersController(ApplicationDbContext db) => _db = db;

    [HasPermission("suppliers.view")]
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Suppliers.OrderBy(x => x.CompanyName).ToListAsync());

    [HasPermission("suppliers.create")]
    [HttpGet("Create")]
    public IActionResult Create() => View(new Supplier());

    [HasPermission("suppliers.create")]
    [HttpPost("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Suppliers.Add(model); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("suppliers.edit")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Suppliers.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HasPermission("suppliers.edit")]
    [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Suppliers.Update(model); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("suppliers.delete")]
    [HttpPost("Delete/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Suppliers.FindAsync(id);
        if (item != null) { _db.Suppliers.Remove(item); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}

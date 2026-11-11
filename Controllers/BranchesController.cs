using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;
using MyPharmacy.Security;

namespace MyPharmacy.Controllers;

[Route("Branches")]
public class BranchesController : Controller
{
    private readonly ApplicationDbContext _db;
    public BranchesController(ApplicationDbContext db) => _db = db;

    [HasPermission("branches.view")]
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Branches.OrderBy(x => x.Name).ToListAsync());

    [HasPermission("branches.create")]
    [HttpGet("Create")]
    public IActionResult Create() => View(new Branch());

    [HasPermission("branches.create")]
    [HttpPost("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Branches.Add(model); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("branches.edit")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Branches.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HasPermission("branches.edit")]
    [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Branches.Update(model); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("branches.delete")]
    [HttpPost("Delete/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Branches.FindAsync(id);
        if (item != null) { _db.Branches.Remove(item); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}

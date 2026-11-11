using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Security;

namespace MyPharmacy.Controllers;

[HasPermission("roles.view")]
[Route("Permissions")]
public class PermissionsController : Controller
{
    private readonly ApplicationDbContext _db;
    public PermissionsController(ApplicationDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(await _db.Permissions.OrderBy(p => p.Name).ToListAsync());
}

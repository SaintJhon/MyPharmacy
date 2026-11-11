using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;
using MyPharmacy.Security;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Controllers;

[Route("Roles")]
public class RolesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RolesController(ApplicationDbContext db, RoleManager<ApplicationRole> roleManager)
    {
        _db = db;
        _roleManager = roleManager;
    }

    [HasPermission("roles.view")]
    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(await _db.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Name)
            .ToListAsync());

    [HasPermission("roles.create")]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadPermissionsAsync();
        return View(new RoleViewModel());
    }

    [HasPermission("roles.create")]
    [HttpPost("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        await LoadPermissionsAsync();

        if (!ModelState.IsValid)
            return View(model);

        var roleName = model.Name.Trim();
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
            return View(model);
        }

        var role = new ApplicationRole
        {
            Name = roleName,
            Description = model.Description?.Trim()
        };

        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        await SavePermissionsAsync(role.Id, model.PermissionIds);
        TempData["Success"] = "Role created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("roles.edit")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _db.Roles
            .Include(item => item.RolePermissions)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (role == null)
            return NotFound();

        await LoadPermissionsAsync();

        return View(new RoleViewModel
        {
            Id = id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            PermissionIds = role.RolePermissions.Select(item => item.PermissionId).ToList()
        });
    }

    [HasPermission("roles.edit")]
    [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoleViewModel model)
    {
        await LoadPermissionsAsync();

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return NotFound();

        var duplicateRole = await _roleManager.FindByNameAsync(model.Name.Trim());
        if (duplicateRole != null && duplicateRole.Id != id)
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
            return View(model);
        }

        role.Name = model.Name.Trim();
        role.Description = model.Description?.Trim();

        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(model);
        }

        await SavePermissionsAsync(id, model.PermissionIds);
        TempData["Success"] = "Role and permissions updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadPermissionsAsync()
    {
        ViewBag.Permissions = await _db.Permissions.OrderBy(permission => permission.Name).ToListAsync();
    }

    private async Task SavePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var selectedIds = permissionIds.Distinct().ToList();
        var validIds = await _db.Permissions
            .Where(permission => selectedIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToListAsync();

        var existing = _db.RolePermissions.Where(item => item.RoleId == roleId);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in validIds)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        await _db.SaveChangesAsync();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}

public class RoleViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public List<int> PermissionIds { get; set; } = new();
}

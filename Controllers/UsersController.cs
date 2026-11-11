using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;
using MyPharmacy.Security;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Controllers;

[Route("Users")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<ApplicationRole> _roles;

    public UsersController(
        UserManager<ApplicationUser> users,
        RoleManager<ApplicationRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    [HasPermission("users.view")]
    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(await _users.Users.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync());

    [HasPermission("users.create")]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();
        return View(new UserCreateViewModel());
    }

    [HasPermission("users.create")]
    [HttpPost("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        await LoadRolesAsync();

        if (!ModelState.IsValid)
            return View(model);

        var validRoles = await GetValidRoleNamesAsync(model.RoleNames);
        if (validRoles.Count != model.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            ModelState.AddModelError(nameof(model.RoleNames), "One or more selected roles are invalid.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var created = await _users.CreateAsync(user, model.Password);

        if (!created.Succeeded)
        {
            AddIdentityErrors(created);
            return View(model);
        }

        if (validRoles.Count > 0)
        {
            var roleResult = await _users.AddToRolesAsync(user, validRoles);

            if (!roleResult.Succeeded)
            {
                await _users.DeleteAsync(user);
                AddIdentityErrors(roleResult);
                return View(model);
            }
        }

        TempData["Success"] = "User account created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("users.edit")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _users.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        await LoadRolesAsync();

        return View(new UserEditViewModel
        {
            Id = id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            RoleNames = (await _users.GetRolesAsync(user)).ToList()
        });
    }

    [HasPermission("users.edit")]
    [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel model)
    {
        await LoadRolesAsync();

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var user = await _users.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var emailOwner = await _users.FindByEmailAsync(model.Email.Trim());
        if (emailOwner != null && emailOwner.Id != id)
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
            return View(model);
        }

        var validRoles = await GetValidRoleNamesAsync(model.RoleNames);
        if (validRoles.Count != model.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            ModelState.AddModelError(nameof(model.RoleNames), "One or more selected roles are invalid.");
            return View(model);
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;

        var update = await _users.UpdateAsync(user);
        if (!update.Succeeded)
        {
            AddIdentityErrors(update);
            return View(model);
        }

        var currentRoles = await _users.GetRolesAsync(user);
        var removeResult = await _users.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            AddIdentityErrors(removeResult);
            return View(model);
        }

        if (validRoles.Count > 0)
        {
            var addResult = await _users.AddToRolesAsync(user, validRoles);
            if (!addResult.Succeeded)
            {
                AddIdentityErrors(addResult);
                return View(model);
            }
        }

        TempData["Success"] = "User information updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HasPermission("users.delete")]
    [HttpPost("Deactivate/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _users.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var currentUserId = _users.GetUserId(User);
        if (int.TryParse(currentUserId, out var parsedCurrentUserId) && user.Id == parsedCurrentUserId)
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = false;
        var result = await _users.UpdateAsync(user);

        if (!result.Succeeded)
            AddIdentityErrors(result);
        else
            TempData["Success"] = "User account deactivated successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync()
    {
        ViewBag.Roles = await _roles.Roles.OrderBy(x => x.Name).ToListAsync();
    }

    private async Task<List<string>> GetValidRoleNamesAsync(IEnumerable<string> requestedNames)
    {
        var requested = requestedNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return await _roles.Roles
            .Where(role => requested.Contains(role.Name!))
            .Select(role => role.Name!)
            .ToListAsync();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}

public class UserCreateViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required, MinLength(8), DataType(DataType.Password)]
    [Display(Name = "Temporary password")]
    public string Password { get; set; } = string.Empty;

    public List<string> RoleNames { get; set; } = new();
}

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Account active")]
    public bool IsActive { get; set; }

    public List<string> RoleNames { get; set; } = new();
}

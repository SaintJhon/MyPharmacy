using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPharmacy.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signInManager)
    {
        _users = users;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Challenge();

        return View(new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var normalizedEmail = model.Email.Trim();
        var emailOwner = await _users.FindByEmailAsync(normalizedEmail);

        if (emailOwner != null && emailOwner.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
            return View(model);
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Password updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}

public class ProfileViewModel
{
    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

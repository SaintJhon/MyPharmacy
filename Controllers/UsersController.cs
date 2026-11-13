using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Controllers
{
    [Route("Users")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // list usua get
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userList = await _userManager.Users.ToListAsync();
            return View(userList);
        }

        // Creat Usua GET
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(new UserCreateViewModel());
        }

        // crear usua POST
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
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

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (model.RoleNames != null && model.RoleNames.Count > 0)
                {
                    await _userManager.AddToRolesAsync(user, model.RoleNames);
                }

                TempData["Success"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }

        // editar usua GEt
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                RoleNames = (await _userManager.GetRolesAsync(user)).ToList()
            };

            return View(model);
        }

        // editar usua Post
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();
            user.UserName = model.Email.Trim();
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var selectedRoles = model.RoleNames ?? new List<string>();

                var rolesToRemove = currentRoles.Except(selectedRoles);
                var rolesToAdd = selectedRoles.Except(currentRoles);

                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }

        // elim usua post
        [HttpPost("Deactivate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "User deactivated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels 
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

        [Required, MinLength(6), DataType(DataType.Password)]
        [Display(Name = "Password")]
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

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public List<string> RoleNames { get; set; } = new();
    }
}
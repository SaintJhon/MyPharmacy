using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Models;

namespace MyPharmacy.Data;

public static class SeedData
{
    private static readonly (string Code, string Name, string Description)[] PermissionDefinitions =
    {
        ("users.view", "View users", "View the user directory."),
        ("users.create", "Create users", "Create new user accounts."),
        ("users.edit", "Edit users", "Edit user information and assigned roles."),
        ("users.delete", "Deactivate users", "Deactivate user accounts."),

        ("roles.view", "View roles", "View roles and their assigned permissions."),
        ("roles.create", "Create roles", "Create new roles."),
        ("roles.edit", "Edit roles", "Edit roles and their assigned permissions."),

        ("products.view", "View products", "View the product catalog."),
        ("products.create", "Create products", "Add products to the catalog."),
        ("products.edit", "Edit products", "Update product information."),
        ("products.delete", "Delete products", "Remove products from the catalog."),

        ("branches.view", "View branches", "View pharmacy branches."),
        ("branches.create", "Create branches", "Add pharmacy branches."),
        ("branches.edit", "Edit branches", "Update pharmacy branch information."),
        ("branches.delete", "Delete branches", "Remove pharmacy branches."),

        ("suppliers.view", "View suppliers", "View supplier information."),
        ("suppliers.create", "Create suppliers", "Add suppliers."),
        ("suppliers.edit", "Edit suppliers", "Update supplier information."),
        ("suppliers.delete", "Delete suppliers", "Remove suppliers.")
    };

    private static readonly string[] EmployeePermissionCodes =
    {
        "products.view",
        "products.create",
        "products.edit",
        "branches.view",
        "suppliers.view"
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var item in PermissionDefinitions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Code == item.Code))
            {
                db.Permissions.Add(new Permission
                {
                    Code = item.Code,
                    Name = item.Name,
                    Description = item.Description
                });
            }
        }

        await db.SaveChangesAsync();

        await EnsureRoleAsync(roleManager, "Administrator", "Full access to all application modules.");
        await EnsureRoleAsync(roleManager, "Employee", "Limited access for day-to-day pharmacy operations.");

        var administratorRole = await roleManager.FindByNameAsync("Administrator")
            ?? throw new InvalidOperationException("The Administrator role could not be created.");

        var employeeRole = await roleManager.FindByNameAsync("Employee")
            ?? throw new InvalidOperationException("The Employee role could not be created.");

        var allPermissionIds = await db.Permissions.Select(p => p.Id).ToListAsync();
        await AddPermissionsAsync(db, administratorRole.Id, allPermissionIds);

        var employeePermissionIds = await db.Permissions
            .Where(p => EmployeePermissionCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync();

        await AddPermissionsAsync(db, employeeRole.Id, employeePermissionIds);

        await EnsureUserAsync(
            userManager,
            email: "admin@mypharmacy.local",
            password: "Admin12345",
            firstName: "System",
            lastName: "Administrator",
            roleName: "Administrator");

        await EnsureUserAsync(
            userManager,
            email: "employee@mypharmacy.local",
            password: "Employee12345",
            firstName: "Alex",
            lastName: "Morgan",
            roleName: "Employee");
    }

    private static async Task EnsureRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string name,
        string description)
    {
        if (!await roleManager.RoleExistsAsync(name))
        {
            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = name,
                Description = description
            });

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create role '{name}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }

    private static async Task AddPermissionsAsync(
        ApplicationDbContext db,
        int roleId,
        IEnumerable<int> permissionIds)
    {
        var existingIds = await db.RolePermissions
            .Where(x => x.RoleId == roleId)
            .Select(x => x.PermissionId)
            .ToListAsync();

        foreach (var permissionId in permissionIds.Except(existingIds))
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create demo user '{email}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Could not assign role '{roleName}' to '{email}': {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Models;

namespace MyPharmacy.Data;

public static class SeedData
{
    private static readonly (string Code, string Name, string Description)[] PermissionDefinitions =
    {
        ("users.view", "Ver usuarios", "Ver el directorio de usuarios."),
        ("users.create", "Crear usuarios", "Crear nuevas cuentas de usuario."),
        ("users.edit", "Editar usuarios", "Editar la información de los usuarios y sus roles asignados."),
        ("users.delete", "Desactivar usuarios", "Desactivar cuentas de usuario."),

        ("roles.view", "Ver roles", "Ver los roles y sus permisos asignados."),
        ("roles.create", "Crear roles", "Crear nuevos roles."),
        ("roles.edit", "Editar roles", "Editar los roles y sus permisos asignados."),

        ("products.view", "Ver productos", "Ver el catálogo de productos."),
        ("products.create", "Crear productos", "Agregar productos al catálogo."),
        ("products.edit", "Editar productos", "Actualizar la información de los productos."),
        ("products.delete", "Eliminar productos", "Eliminar productos del catálogo."),

        ("branches.view", "Ver sucursales", "Ver las sucursales de la farmacia."),
        ("branches.create", "Crear sucursales", "Agregar sucursales de la farmacia."),
        ("branches.edit", "Editar sucursales", "Actualizar la información de las sucursales."),
        ("branches.delete", "Eliminar sucursales", "Eliminar sucursales de la farmacia."),

        ("suppliers.view", "Ver proveedores", "Ver la información de los proveedores."),
        ("suppliers.create", "Crear proveedores", "Agregar proveedores."),
        ("suppliers.edit", "Editar proveedores", "Actualizar la información de los proveedores."),
        ("suppliers.delete", "Eliminar proveedores", "Eliminar proveedores.")
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

        await EnsureRoleAsync(roleManager, "Administrador", "Acceso completo a todos los módulos de la aplicación.");
        await EnsureRoleAsync(roleManager, "Empleado", "Acceso limitado para las operaciones diarias de la farmacia.");

        var administratorRole = await roleManager.FindByNameAsync("Administrador")
            ?? throw new InvalidOperationException("No se pudo crear el rol Administrador.");

        var employeeRole = await roleManager.FindByNameAsync("Empleado")
            ?? throw new InvalidOperationException("No se pudo crear el rol Empleado.");

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
            roleName: "Administrador");

        await EnsureUserAsync(
            userManager,
            email: "employee@mypharmacy.local",
            password: "Employee12345",
            firstName: "Alex",
            lastName: "Morgan",
            roleName: "Empleado");
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
                    $"No se pudo crear el rol '{name}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
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
                    $"No se pudo crear el usuario de demostración '{email}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"No se pudo asignar el rol '{roleName}' a '{email}': {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
        }
    }
}

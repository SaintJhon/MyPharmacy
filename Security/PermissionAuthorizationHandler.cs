using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Data;
using MyPharmacy.Models;

namespace MyPharmacy.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public PermissionAuthorizationHandler(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = await _users.GetUserAsync(context.User);
        if (user is null || !user.IsActive) return;

        var hasPermission = await (
            from userRole in _db.UserRoles
            join rolePermission in _db.RolePermissions
                on userRole.RoleId equals rolePermission.RoleId
            join permission in _db.Permissions
                on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == user.Id && permission.Code == requirement.Code
            select permission.Id
        ).AnyAsync();

        if (hasPermission)
            context.Succeed(requirement);
    }
}

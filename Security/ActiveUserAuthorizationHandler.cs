using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyPharmacy.Models;

namespace MyPharmacy.Security;

public sealed class ActiveUserAuthorizationHandler : AuthorizationHandler<ActiveUserRequirement>
{
    private readonly UserManager<ApplicationUser> _users;

    public ActiveUserAuthorizationHandler(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var user = await _users.GetUserAsync(context.User);

        if (user is { IsActive: true })
            context.Succeed(requirement);
    }
}

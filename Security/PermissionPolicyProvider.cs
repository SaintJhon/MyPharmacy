using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyPharmacy.Security;

public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(HasPermissionAttribute.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var code = policyName[HasPermissionAttribute.Prefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ActiveUserRequirement())
                .AddRequirements(new PermissionRequirement(code))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}

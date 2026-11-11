using Microsoft.AspNetCore.Authorization;

namespace MyPharmacy.Security;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;

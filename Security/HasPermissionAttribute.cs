using Microsoft.AspNetCore.Authorization;

namespace MyPharmacy.Security;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string Prefix = "PERMISSION:";
    public HasPermissionAttribute(string permissionCode) => Policy = Prefix + permissionCode;
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Models;

public class ApplicationRole : IdentityRole<int>
{
    [StringLength(250)]
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

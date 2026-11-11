using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Models;

public class Permission
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

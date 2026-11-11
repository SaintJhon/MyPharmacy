using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Models;

public class ApplicationUser : IdentityUser<int>
{
    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

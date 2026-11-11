using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Nit { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Contact { get; set; }

    [StringLength(25)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
}

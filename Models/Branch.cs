using System.ComponentModel.DataAnnotations;

namespace MyPharmacy.Models;

public class Branch
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(25)]
    public string Phone { get; set; } = string.Empty;
}

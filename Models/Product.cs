using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPharmacy.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999)]
    public decimal SalePrice { get; set; }

    public DateTime ExpirationDate { get; set; }
}

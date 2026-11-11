using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPharmacy.Models;

namespace MyPharmacy.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Entity<RolePermission>()
            .HasOne(x => x.Role).WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<RolePermission>()
            .HasOne(x => x.Permission).WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Permission>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Barcode).IsUnique();
        builder.Entity<Supplier>().HasIndex(x => x.Nit).IsUnique();
    }
}

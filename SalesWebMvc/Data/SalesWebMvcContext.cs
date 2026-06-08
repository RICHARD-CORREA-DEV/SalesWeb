using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Models;

public class SalesWebMvcContext(DbContextOptions<SalesWebMvcContext> options) : DbContext(options)
{
    public DbSet<SalesWebMvc.Models.Department> Department { get; set; } = default!;
    public required DbSet<Seller> Seller { get; set; } = default!;
    public required DbSet<SalesRecord> SalesRecord { get; set; } = default!;
}

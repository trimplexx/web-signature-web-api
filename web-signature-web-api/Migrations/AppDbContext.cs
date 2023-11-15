using web_signature_web_api.Models;
using Microsoft.EntityFrameworkCore;

namespace web_signature_web_api.Migrations;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial " +
                                    "Catalog=Signature;Integrated " +
                                    "Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True");
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserPublicKey> UserPublicKeys { get; set; }
}
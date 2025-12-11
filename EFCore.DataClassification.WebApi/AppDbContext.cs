using Microsoft.EntityFrameworkCore;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.WebApi.Models;

namespace EFCore.DataClassification.WebApi;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Car> Car { get; set; }
    public DbSet<Bike> Bikes { get; set; }
    public DbSet<Home> Homes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // 1. Scan and apply DataClassification attributes
        modelBuilder.UseDataClassification();

        // 2. Fluent API configuration example
        modelBuilder.Entity<User>()
            .Property(u => u.PhoneNumber)
            .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);
    }
}
using Microsoft.EntityFrameworkCore;
using EFCore.DataClassification.Extensions; // UseDataClassification ve HasDataClassification buradan gelir
using EFCore.DataClassification.Models;     // SensitivityRank buradan gelir
using EFCore.DataClassification.WebApi.Models; // User sınıfı buradan gelir

namespace EFCore.DataClassification.WebApi;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // 1. Senin kütüphaneni çalıştır (Attribute tarayıcı)
        modelBuilder.UseDataClassification();

        // 2. Fluent API Testi
        modelBuilder.Entity<User>()
            .Property(u => u.PhoneNumber)
            .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);
    }
}
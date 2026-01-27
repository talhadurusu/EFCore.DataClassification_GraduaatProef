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



    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.UseDataClassification();

        // 2. Fluent API configuration 
        //modelBuilder.Entity<User>()
        //    .Property(u => u.PhoneNumber)
        //    .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);


        //  Relationship configurations
        modelBuilder.Entity<User>()
            .HasOne(u => u.Admin)
            .WithMany(a => a.Users)
            .HasForeignKey(u => u.AdminId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.User)
            .WithMany(u => u.Games)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
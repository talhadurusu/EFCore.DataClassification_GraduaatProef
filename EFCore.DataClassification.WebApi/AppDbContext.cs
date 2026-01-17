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
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<PersonBase> People { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Contractor> Contractors { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // 1. Scan and apply DataClassification attributes
        modelBuilder.UseDataClassification();

        // 2. Fluent API configuration example
        modelBuilder.Entity<User>()
            .Property(u => u.PhoneNumber)
            .HasDataClassification("Internal", "Phone Number", SensitivityRank.High);

        modelBuilder.Entity<User>()
            .Property<string>("ShadowSecret")
            .HasDataClassification("Security", "Shadow Secret", SensitivityRank.High);

        modelBuilder.Entity<User>()
            .Property(u => u.AccountStatus)
            .HasColumnName("Status");

        modelBuilder.Entity<Admin>()
            .Property(a => a.InscriptionNumber)
            .HasColumnName("RegistrationNumber");

        modelBuilder.Entity<Home>()
            .Property(h => h.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PersonBase>()
            .ToTable("People");

        modelBuilder.Entity<Employee>()
            .ToTable("Employees");

        modelBuilder.Entity<Contractor>()
            .ToTable("Contractors");

        modelBuilder.Entity<Employee>()
            .Property(e => e.Salary)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PersonBase>()
            .OwnsOne(p => p.Contact, owned => {
                owned.Property(p => p.Email).HasColumnName("ContactEmail");
                owned.Property(p => p.Phone).HasColumnName("ContactPhone");
            });
    }
}
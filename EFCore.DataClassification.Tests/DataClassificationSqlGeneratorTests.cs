using System;
using System.Linq;
using EFCore.DataClassification.Constants;
using EFCore.DataClassification.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.DataClassification.Tests {
    // Basit bir test entity
    public class Admin {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
    }

    // Test için DbContext: annotation'ları burada veriyoruz
    public class TestClassificationContext : DbContext {
        public DbSet<Admin> Admins => Set<Admin>();

        public TestClassificationContext(DbContextOptions<TestClassificationContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Admin>(b => {
                b.ToTable("Admins", "dbo");

                // Email kolonuna DataClassification annotation'ları ekliyoruz
                b.Property(a => a.Email)
                    .HasAnnotation(DataClassificationConstants.Label, "Private")
                    .HasAnnotation(DataClassificationConstants.InformationType, "ContactInfo")
                    .HasAnnotation(DataClassificationConstants.Rank, "High");

                // Phone için de annotation verelim (Add/Alter testlerinde kullanacağız)
                b.Property(a => a.Phone)
                    .HasAnnotation(DataClassificationConstants.Label, "Sensitive")
                    .HasAnnotation(DataClassificationConstants.InformationType, "PhoneNumber")
                    .HasAnnotation(DataClassificationConstants.Rank, "Medium");
            });
        }
    }

    public class DataClassificationSqlGeneratorTests {
        private (DataClassificationSqlGenerator Generator,
                 MigrationsSqlGeneratorDependencies Dependencies,
                 IModel Model)
            CreateSut() {
            // EF Core servisleri
            var services = new ServiceCollection()
                .AddLogging()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            // DbContext options
            var options = new DbContextOptionsBuilder<TestClassificationContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Dummy;Trusted_Connection=True;")
                .UseInternalServiceProvider(services)
                .Options;

            using var ctx = new TestClassificationContext(options);
            var model = ctx.Model; // IModel

            var deps = services.GetRequiredService<MigrationsSqlGeneratorDependencies>();
            var batchPreparer = services.GetRequiredService<ICommandBatchPreparer>();

            var generator = new DataClassificationSqlGenerator(deps, batchPreparer);

            return (generator, deps, model);
        }

        private static string BuildSql(
            DataClassificationSqlGenerator generator,
            MigrationsSqlGeneratorDependencies deps,
            MigrationOperation operation,
            IModel? model = null) {
            var builder = new MigrationCommandListBuilder(deps);

            switch (operation) {
                case CreateTableOperation cto:
                    generator.Generate(cto, model, builder);
                    break;
                case AddColumnOperation add:
                    generator.Generate(add, model, builder);
                    break;
                case AlterColumnOperation alt:
                    generator.Generate(alt, model, builder);
                    break;
                case DropColumnOperation drop:
                    generator.Generate(drop, model, builder, terminate: true);
                    break;
                default:
                    throw new NotSupportedException($"Operation type {operation.GetType().Name} not supported in test helper.");
            }

            var commands = builder.GetCommandList();
            return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
        }

        [Fact]
        public void CreateTable_Generates_ExtendedProperties_And_SensitivityClassification() {
            // Arrange
            var (generator, deps, model) = CreateSut();

            var create = new CreateTableOperation {
                Name = "Admins",
                Schema = "dbo"
            };

            create.Columns.Add(new AddColumnOperation {
                Name = "Id",
                Table = "Admins",
                Schema = "dbo",
                ClrType = typeof(int),
                ColumnType = "int",
                IsNullable = false
            });

            create.Columns.Add(new AddColumnOperation {
                Name = "Email",
                Table = "Admins",
                Schema = "dbo",
                ClrType = typeof(string),
                ColumnType = "nvarchar(max)",
                IsNullable = false
            });

            // Act
            var sql = BuildSql(generator, deps, create, model);

            // Assert
            // Normal CREATE TABLE
            Assert.Contains("CREATE TABLE [dbo].[Admins]", sql, StringComparison.OrdinalIgnoreCase);

            // Extended properties
            Assert.Contains("sp_addextendedproperty", sql);
            Assert.Contains(DataClassificationConstants.Label, sql);
            Assert.Contains("Private", sql);

            // Sensitivity classification
            Assert.Contains("ADD SENSITIVITY CLASSIFICATION TO [dbo].[Admins].[Email]", sql);
            Assert.Contains("LABEL = N'Private'", sql);
            Assert.Contains("INFORMATION_TYPE = N'ContactInfo'", sql);
            Assert.Contains("RANK = HIGH", sql);
        }

        [Fact]
        public void AddColumn_Generates_Classifications_For_New_Column() {
            // Arrange
            var (generator, deps, model) = CreateSut();

            var addPhone = new AddColumnOperation {
                Table = "Admins",
                Schema = "dbo",
                Name = "Phone",
                ClrType = typeof(string),
                ColumnType = "nvarchar(max)",
                IsNullable = true
            };

            // Act
            var sql = BuildSql(generator, deps, addPhone, model);

            // Assert
            Assert.Contains("ALTER TABLE [dbo].[Admins] ADD [Phone]", sql);
            Assert.Contains("ADD SENSITIVITY CLASSIFICATION TO [dbo].[Admins].[Phone]", sql);
            Assert.Contains("LABEL = N'Sensitive'", sql);
            Assert.Contains("INFORMATION_TYPE = N'PhoneNumber'", sql);
            Assert.Contains("RANK = MEDIUM", sql);
        }

        [Fact]
        public void AlterColumn_Drops_And_Recreates_Classification() {
            // Arrange
            var (generator, deps, model) = CreateSut();

            var alterEmail = new AlterColumnOperation {
                Table = "Admins",
                Schema = "dbo",
                Name = "Email",
                ClrType = typeof(string),
                ColumnType = "nvarchar(200)",
                IsNullable = false,
                OldColumn = new ColumnOperation {
                    Table = "Admins",
                    Schema = "dbo",
                    ClrType = typeof(string),
                    ColumnType = "nvarchar(max)",
                    IsNullable = false
                }
            };

            // Act
            var sql = BuildSql(generator, deps, alterEmail, model);

            // Assert
            // Normal ALTER COLUMN
            Assert.Contains("ALTER TABLE [dbo].[Admins] ALTER COLUMN [Email]", sql);

            // Önce drop sensitivity / extended properties (ClearDataClassification)
            Assert.Contains("DROP SENSITIVITY CLASSIFICATION FROM [dbo].[Admins].[Email]", sql);

            // Sonra yeniden ADD SENSITIVITY CLASSIFICATION
            Assert.Contains("ADD SENSITIVITY CLASSIFICATION TO [dbo].[Admins].[Email]", sql);
            Assert.Contains("RANK = HIGH", sql);
        }

        [Fact]
        public void DropColumn_Clears_Classification_Before_Drop() {
            // Arrange
            var (generator, deps, _) = CreateSut();

            var dropPhone = new DropColumnOperation {
                Table = "Admins",
                Schema = "dbo",
                Name = "Phone"
            };

            // Act
            var sql = BuildSql(generator, deps, dropPhone, model: null);

            // Assert
            // IF EXISTS ... DROP SENSITIVITY CLASSIFICATION ...
            Assert.Contains("DROP SENSITIVITY CLASSIFICATION FROM [dbo].[Admins].[Phone]", sql);

            // Sonrasında normal DROP COLUMN
            Assert.Contains("ALTER TABLE [dbo].[Admins] DROP COLUMN [Phone]", sql);
        }
    }
}

using System;
using System.Linq;
using EFCore.DataClassification.Infrastructure;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.DataClassification.Tests.Integration {

    #pragma warning disable EF1001 
    public class DataClassificationIntegrationTests {
        private const string ConnectionString =
            "Server=(localdb)\\mssqllocaldb;Database=Fake;Trusted_Connection=True;TrustServerCertificate=True";

       
        // New entity with a classified property
        [Fact]
        public void New_entity_with_classified_property_produces_CreateDataClassificationOperation() {
            // Arrange
            var (sourceModel, targetModel) =
                BuildModels<SourceContext_Empty, TargetContext_NewClassifiedEntity>();

            var differ = CreateDiffer();

            // Act
            var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

            // Assert
            var createOp = Assert.Single(
                operations.OfType<CreateDataClassificationOperation>());

            Assert.Equal("dbo", createOp.Schema);
            Assert.Equal("Users", createOp.Table);
            Assert.Equal("Email", createOp.Column);
            Assert.Equal("Confidential", createOp.Label);
            Assert.Equal("Email Address", createOp.InformationType);
            Assert.Equal("High", createOp.Rank);
        }

        // Existing table, new classified column
        [Fact]
        public void Existing_table_new_classified_column_produces_CreateDataClassificationOperation() {
            // Arrange
            var (sourceModel, targetModel) =
                BuildModels<SourceContext_Users_NoEmail, TargetContext_Users_WithClassifiedEmail>();

            var differ = CreateDiffer();

            // Act
            var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

            // Assert
            Assert.Contains(operations, o =>
                o is AddColumnOperation add &&
                add.Table == "Users" &&
                add.Name == "Email");

            var createOp = Assert.Single(
                operations.OfType<CreateDataClassificationOperation>());

            Assert.Equal("dbo", createOp.Schema);
            Assert.Equal("Users", createOp.Table);
            Assert.Equal("Email", createOp.Column);
            Assert.Equal("Confidential", createOp.Label);
            Assert.Equal("Email Address", createOp.InformationType);
            Assert.Equal("High", createOp.Rank);
        }

        [Fact]
        public void Removing_classification_on_existing_column_emits_remove_operation() {
           
            var (sourceModel, targetModel) =
                BuildModels<TargetContext_Users_WithClassifiedEmail, SourceContext_Users_NoEmail>();

            var differ = CreateDiffer();

            // Act
            var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

            // Assert: RemoveDataClassificationOperation var mı ve doğru mu?
            var removeOp = Assert.Single(
                operations.OfType<RemoveDataClassificationOperation>());

            Assert.Equal("dbo", removeOp.Schema);
            Assert.Equal("Users", removeOp.Table);
            Assert.Equal("Email", removeOp.Column);
        }

        [Fact]
        public void Removing_classified_column_emits_remove_and_drop_operations() {
            // Arrange
            var (sourceModel, targetModel) =
                BuildModels<TargetContext_Users_WithClassifiedEmail, SourceContext_Users_NoEmail>();

            var differ = CreateDiffer();

            // Act
            var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

            // Assert
            var removeOp = operations.OfType<RemoveDataClassificationOperation>().Single();
            var dropOp = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("dbo", removeOp.Schema);
            Assert.Equal("Users", removeOp.Table);
            Assert.Equal("Email", removeOp.Column);

            Assert.Equal("Users", dropOp.Table);
            Assert.Equal("Email", dropOp.Name);

            // RemoveDataClassificationOperation must come first 
            var removeIndex = operations.IndexOf(removeOp);
            var dropIndex = operations.IndexOf(dropOp);

            
            Assert.True(removeIndex >= 0 && dropIndex >= 0, "Remove and Drop operations must exist in the operations list.");
            Assert.True(removeIndex < dropIndex, $"RemoveDataClassificationOperation should come before DropColumnOperation. Actual order: remove={removeIndex}, drop={dropIndex}");
        }

        // Changing classification on existing column
        [Fact]
        public void Changing_classification_on_existing_column_emits_remove_then_create() {
            // Arrange
            var (sourceModel, targetModel) =
                BuildModels<SourceContext_Users_PublicEmail, TargetContext_Users_ConfidentialEmail>();

            var differ = CreateDiffer();

            // Act
            var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

            var remove = operations.OfType<RemoveDataClassificationOperation>().Single();
            var create = operations.OfType<CreateDataClassificationOperation>().Single();

            Assert.Equal("dbo", remove.Schema);
            Assert.Equal("Users", remove.Table);
            Assert.Equal("Email", remove.Column);

            Assert.Equal("dbo", create.Schema);
            Assert.Equal("Users", create.Table);
            Assert.Equal("Email", create.Column);
            Assert.Equal("Confidential", create.Label);
            Assert.Equal("Email Address", create.InformationType);
            Assert.Equal("High", create.Rank);

            // order: remove first, then create
            Assert.True(
                operations.IndexOf(remove) < operations.IndexOf(create),
                "RemoveDataClassificationOperation should come before CreateDataClassificationOperation");
        }

        // Helper: build models using DESIGN-TIME MODEL 
    
        private static (IRelationalModel source, IRelationalModel target)
            BuildModels<TSourceContext, TTargetContext>()
            where TSourceContext : DbContext, new()
            where TTargetContext : DbContext, new() {
            using var sourceContext = new TSourceContext();
            using var targetContext = new TTargetContext();

           
            var sourceModel = sourceContext
                .GetService<IDesignTimeModel>()
                .Model
                .GetRelationalModel();

            var targetModel = targetContext
                .GetService<IDesignTimeModel>()
                .Model
                .GetRelationalModel();

            return (sourceModel, targetModel);
        }

        // Helper: DataClassificationMigrationsModelDiffer instance

        private static DataClassificationMigrationsModelDiffer CreateDiffer() {
          
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(ConnectionString)
                .Options;

            using var context = new DbContext(options);
            var sp = ((IInfrastructure<IServiceProvider>)context).Instance;

            var typeMappingSource = sp.GetRequiredService<IRelationalTypeMappingSource>();
            var migrationsAnnotationProvider = sp.GetRequiredService<IMigrationsAnnotationProvider>();
            var rowIdentityMapFactory = sp.GetRequiredService<IRowIdentityMapFactory>();
            var commandBatchDeps = sp.GetRequiredService<CommandBatchPreparerDependencies>();

            return new DataClassificationMigrationsModelDiffer(
                typeMappingSource,
                migrationsAnnotationProvider,
                rowIdentityMapFactory,
                commandBatchDeps);
        }

 
        // Test DbContext
        

        private abstract class BaseContext : DbContext {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(ConnectionString);
        }

        // 1) Source
        private sealed class SourceContext_Empty : BaseContext {
        }

        // 1) Target: entity + classified property
        private sealed class TargetContext_NewClassifiedEntity : BaseContext {
            public DbSet<UserEntity> Users => Set<UserEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<UserEntity>(b => {
                    b.ToTable("Users", "dbo");
                    b.HasKey(e => e.Id);

                    b.Property(e => e.Email)
                        .HasAnnotation("DataClassification:Label", "Confidential")
                        .HasAnnotation("DataClassification:InformationType", "Email Address")
                        .HasAnnotation("DataClassification:Rank", "High");
                });
            }
        }

        // 2) Source: There is a Users table but no Email column
        private sealed class SourceContext_Users_NoEmail : BaseContext {
            public DbSet<UserEntity_NoEmail> Users => Set<UserEntity_NoEmail>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<UserEntity_NoEmail>(b => {
                    b.ToTable("Users", "dbo");
                    b.HasKey(e => e.Id);
                });
            }
        }

        // 2) Target: Users + new classified Email 
        private sealed class TargetContext_Users_WithClassifiedEmail : BaseContext {
            public DbSet<UserEntity> Users => Set<UserEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<UserEntity>(b => {
                    b.ToTable("Users", "dbo");
                    b.HasKey(e => e.Id);

                    b.Property(e => e.Email)
                        .HasAnnotation("DataClassification:Label", "Confidential")
                        .HasAnnotation("DataClassification:InformationType", "Email Address")
                        .HasAnnotation("DataClassification:Rank", "High");
                });
            }
        }

        // 3) Source: Email Public
        private sealed class SourceContext_Users_PublicEmail : BaseContext {
            public DbSet<UserEntity> Users => Set<UserEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<UserEntity>(b => {
                    b.ToTable("Users", "dbo");
                    b.HasKey(e => e.Id);

                    b.Property(e => e.Email)
                        .HasAnnotation("DataClassification:Label", "Public")
                        .HasAnnotation("DataClassification:InformationType", "Email Address")
                        .HasAnnotation("DataClassification:Rank", "Low");
                });
            }
        }

        // 3) Target: Email Confidential / High
        private sealed class TargetContext_Users_ConfidentialEmail : BaseContext {
            public DbSet<UserEntity> Users => Set<UserEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<UserEntity>(b => {
                    b.ToTable("Users", "dbo");
                    b.HasKey(e => e.Id);

                    b.Property(e => e.Email)
                        .HasAnnotation("DataClassification:Label", "Confidential")
                        .HasAnnotation("DataClassification:InformationType", "Email Address")
                        .HasAnnotation("DataClassification:Rank", "High");
                });
            }
        }

        // Simple entity types
        private sealed class UserEntity {
            public int Id { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        private sealed class UserEntity_NoEmail {
            public int Id { get; set; }
        }
    }
    
}

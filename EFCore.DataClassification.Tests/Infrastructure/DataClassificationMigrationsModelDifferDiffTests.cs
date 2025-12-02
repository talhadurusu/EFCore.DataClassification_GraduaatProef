using System.Linq;
using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.DataClassification.Tests.Infrastructure;

public class DataClassificationMigrationsModelDifferDiffTests {
    private const string Cs = "Server=.;Database=Dummy;Trusted_Connection=True;TrustServerCertificate=True";

    private static TContext CreateCtx<TContext>() where TContext : DbContext {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(Cs)
            .UseDataClassificationSqlServer()
            .Options;

        return (TContext)System.Activator.CreateInstance(typeof(TContext), options)!;
    }

    private static (Microsoft.EntityFrameworkCore.Metadata.IRelationalModel rel, IMigrationsModelDiffer differ)
        GetRelModelAndDiffer(DbContext ctx) {
        var designTimeModel = ctx.GetService<IDesignTimeModel>().Model;
        var rel = designTimeModel.GetRelationalModel();
        var differ = ctx.GetService<IMigrationsModelDiffer>();
        return (rel, differ);
    }

    [Fact]
    public void Diff_WhenClassificationAdded_produces_CreateDataClassificationOperation() {
        using var sourceCtx = CreateCtx<Ctx_NoClassification>();
        using var targetCtx = CreateCtx<Ctx_WithClassification>();

        var (sourceRel, differ) = GetRelModelAndDiffer(sourceCtx);
        var (targetRel, _) = GetRelModelAndDiffer(targetCtx);

        var ops = differ.GetDifferences(sourceRel, targetRel).ToList();

        var create = Assert.Single(ops.OfType<CreateDataClassificationOperation>());
        Assert.Equal("dbo", create.Schema);
        Assert.Equal("Users", create.Table);
        Assert.Equal("Email", create.Column);
        Assert.Equal("Confidential", create.Label);
        Assert.Equal("Email Address", create.InformationType);
        Assert.Equal("High", create.Rank);
    }

    [Fact]
    public void Diff_WhenClassificationRemoved_produces_RemoveDataClassificationOperation() {
        using var sourceCtx = CreateCtx<Ctx_WithClassification>();
        using var targetCtx = CreateCtx<Ctx_NoClassification>();

        var (sourceRel, differ) = GetRelModelAndDiffer(sourceCtx);
        var (targetRel, _) = GetRelModelAndDiffer(targetCtx);

        var ops = differ.GetDifferences(sourceRel, targetRel).ToList();

        var remove = Assert.Single(ops.OfType<RemoveDataClassificationOperation>());
        Assert.Equal("dbo", remove.Schema);
        Assert.Equal("Users", remove.Table);
        Assert.Equal("Email", remove.Column);
    }

    [Fact]
    public void Diff_WhenClassificationChanged_produces_Remove_then_Create() {
        using var sourceCtx = CreateCtx<Ctx_RankLow>();
        using var targetCtx = CreateCtx<Ctx_RankHigh>();

        var (sourceRel, differ) = GetRelModelAndDiffer(sourceCtx);
        var (targetRel, _) = GetRelModelAndDiffer(targetCtx);

        var ops = differ.GetDifferences(sourceRel, targetRel).ToList();

        Assert.Single(ops.OfType<RemoveDataClassificationOperation>());
        var create = Assert.Single(ops.OfType<CreateDataClassificationOperation>());
        Assert.Equal("High", create.Rank);
    }

    // ---- DbContexts ----

    private class User {
        public int Id { get; set; }
        public string Email { get; set; } = "";
    }

    private class BaseCtx<T> : DbContext where T : DbContext {
        public BaseCtx(DbContextOptions<T> options) : base(options) { }
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.UseDataClassification();

            modelBuilder.Entity<User>(b => {
                b.ToTable("Users", "dbo");
                b.Property(x => x.Email).HasColumnName("Email");
            });
        }
    }

    private sealed class Ctx_NoClassification : BaseCtx<Ctx_NoClassification> {
        public Ctx_NoClassification(DbContextOptions<Ctx_NoClassification> options) : base(options) { }
    }

    private sealed class Ctx_WithClassification : BaseCtx<Ctx_WithClassification> {
        public Ctx_WithClassification(DbContextOptions<Ctx_WithClassification> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasDataClassification("Confidential", "Email Address", SensitivityRank.High);
        }
    }

    private sealed class Ctx_RankLow : BaseCtx<Ctx_RankLow> {
        public Ctx_RankLow(DbContextOptions<Ctx_RankLow> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasDataClassification("Confidential", "Email Address", SensitivityRank.Low);
        }
    }

    private sealed class Ctx_RankHigh : BaseCtx<Ctx_RankHigh> {
        public Ctx_RankHigh(DbContextOptions<Ctx_RankHigh> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasDataClassification("Confidential", "Email Address", SensitivityRank.High);
        }
    }
}

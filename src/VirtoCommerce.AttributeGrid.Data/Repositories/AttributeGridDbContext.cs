using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.AttributeGrid.Data.Models;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.AttributeGrid.Data.Repositories;

public class AttributeGridDbContext : DbContextBase
{
    public AttributeGridDbContext(DbContextOptions<AttributeGridDbContext> options)
        : base(options)
    {
    }

    public DbSet<PropertyTrashEntity> PropertyTrashEntries { get; set; }

    protected AttributeGridDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PropertyTrashEntity>(entity =>
        {
            entity.ToTable("AttributeGrid_Trash");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(IdLength);
            entity.Property(x => x.PropertyId).HasMaxLength(IdLength);
            entity.Property(x => x.PropertyName).HasMaxLength(256);
            entity.Property(x => x.PropertyCode).HasMaxLength(128);
            entity.Property(x => x.CatalogId).HasMaxLength(IdLength).IsRequired(false);
            entity.Property(x => x.CategoryId).HasMaxLength(IdLength).IsRequired(false);
            entity.Property(x => x.DeletedBy).HasMaxLength(128);
            entity.HasIndex(x => x.DeletedDate);
        });

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.AttributeGrid.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.AttributeGrid.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.AttributeGrid.Data.SqlServer"));
                break;
        }
    }
}

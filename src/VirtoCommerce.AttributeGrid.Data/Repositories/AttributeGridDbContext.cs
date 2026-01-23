using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.AttributeGrid.Data.Repositories;

public class AttributeGridDbContext : DbContextBase
{
    public AttributeGridDbContext(DbContextOptions<AttributeGridDbContext> options)
        : base(options)
    {
    }

    protected AttributeGridDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<AttributeGridEntity>().ToTable("AttributeGrid").HasKey(x => x.Id);
        //modelBuilder.Entity<AttributeGridEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

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

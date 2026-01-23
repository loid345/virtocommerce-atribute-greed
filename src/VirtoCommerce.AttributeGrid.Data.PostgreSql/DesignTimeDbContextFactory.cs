using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.AttributeGrid.Data.Repositories;

namespace VirtoCommerce.AttributeGrid.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttributeGridDbContext>
{
    public AttributeGridDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AttributeGridDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(PostgreSqlDataAssemblyMarker).Assembly.GetName().Name));

        return new AttributeGridDbContext(builder.Options);
    }
}

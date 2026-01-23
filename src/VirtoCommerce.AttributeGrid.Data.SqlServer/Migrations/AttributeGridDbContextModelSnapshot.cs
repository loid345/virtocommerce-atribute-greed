using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using VirtoCommerce.AttributeGrid.Data.Models;
using VirtoCommerce.AttributeGrid.Data.Repositories;

#nullable disable

namespace VirtoCommerce.AttributeGrid.Data.SqlServer.Migrations;

[DbContext(typeof(AttributeGridDbContext))]
partial class AttributeGridDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0");

        modelBuilder.Entity<PropertyTrashEntity>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.PropertyId).HasMaxLength(128);
            entity.Property(e => e.PropertyName).HasMaxLength(256);
            entity.Property(e => e.PropertyCode).HasMaxLength(128);
            entity.Property(e => e.CatalogId).HasMaxLength(128);
            entity.Property(e => e.CategoryId).HasMaxLength(128);
            entity.Property(e => e.DeletedBy).HasMaxLength(128);

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeletedDate);
            entity.ToTable("AttributeGrid_Trash");
        });
    }
}

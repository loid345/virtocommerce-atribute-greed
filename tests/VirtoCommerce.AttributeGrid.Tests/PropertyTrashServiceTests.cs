using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using VirtoCommerce.AttributeGrid.Data.Models;
using VirtoCommerce.AttributeGrid.Data.Repositories;
using VirtoCommerce.AttributeGrid.Data.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using Xunit;

namespace VirtoCommerce.AttributeGrid.Tests;

[Trait("Category", "Unit")]
public class PropertyTrashServiceTests
{
    private static AttributeGridDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AttributeGridDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AttributeGridDbContext(options);
    }

    [Fact]
    public async Task MoveToTrash_ShouldCreateTrashEntry()
    {
        var context = CreateInMemoryContext();
        var mockPropertyService = new Mock<IPropertyService>();

        var testProperty = new Property
        {
            Id = "prop-123",
            Name = "Test Property",
            CatalogId = "catalog-1",
        };

        mockPropertyService
            .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), null))
            .ReturnsAsync(new[] { testProperty });

        mockPropertyService
            .Setup(x => x.DeleteAsync(It.IsAny<string[]>()))
            .Returns(Task.CompletedTask);

        var service = new PropertyTrashService(
            () => context,
            mockPropertyService.Object);

        await service.MoveToTrashAsync("prop-123", "admin");

        var trashEntries = await context.PropertyTrashEntries.ToListAsync();
        Assert.Single(trashEntries);
        Assert.Equal("prop-123", trashEntries[0].PropertyId);
        Assert.Equal("Test Property", trashEntries[0].PropertyName);
        Assert.Equal("admin", trashEntries[0].DeletedBy);
    }

    [Fact]
    public async Task GetTrashEntries_ShouldNotReturnExpired()
    {
        var context = CreateInMemoryContext();

        context.PropertyTrashEntries.Add(new PropertyTrashEntity
        {
            Id = "1",
            PropertyId = "p1",
            PropertyName = "Active",
            DeletedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
        });

        context.PropertyTrashEntries.Add(new PropertyTrashEntity
        {
            Id = "2",
            PropertyId = "p2",
            PropertyName = "Expired",
            DeletedDate = DateTime.UtcNow.AddDays(-60),
            ExpirationDate = DateTime.UtcNow.AddDays(-30),
        });

        await context.SaveChangesAsync();

        var mockPropertyService = new Mock<IPropertyService>();
        var service = new PropertyTrashService(() => context, mockPropertyService.Object);

        var result = await service.GetTrashEntriesAsync();

        Assert.Single(result);
        Assert.Equal("Active", result[0].PropertyName);
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldRemoveFromTrash()
    {
        var context = CreateInMemoryContext();

        context.PropertyTrashEntries.Add(new PropertyTrashEntity
        {
            Id = "trash-1",
            PropertyId = "p1",
            PropertyName = "Restore Me",
            PropertyDataJson = "{\"Id\":\"p1\",\"Name\":\"Restore Me\"}",
            DeletedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
        });

        await context.SaveChangesAsync();

        var mockPropertyService = new Mock<IPropertyService>();
        mockPropertyService
            .Setup(x => x.SaveChangesAsync(It.IsAny<Property[]>()))
            .Returns(Task.CompletedTask);

        var service = new PropertyTrashService(() => context, mockPropertyService.Object);

        await service.RestoreFromTrashAsync("trash-1");

        var remaining = await context.PropertyTrashEntries.ToListAsync();
        Assert.Empty(remaining);
        mockPropertyService.Verify(x => x.SaveChangesAsync(It.IsAny<Property[]>()), Times.Once);
    }
}

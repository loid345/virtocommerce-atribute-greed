using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.AttributeGrid.Data.Models;
using VirtoCommerce.AttributeGrid.Data.Repositories;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.AttributeGrid.Data.Services;

public class PropertyTrashService : IPropertyTrashService
{
    private readonly Func<AttributeGridDbContext> _dbContextFactory;
    private readonly IPropertyService _propertyService;

    public PropertyTrashService(Func<AttributeGridDbContext> dbContextFactory, IPropertyService propertyService)
    {
        _dbContextFactory = dbContextFactory;
        _propertyService = propertyService;
    }

    public async Task<IList<PropertyTrashEntry>> GetTrashEntriesAsync()
    {
        await using var context = _dbContextFactory();

        return await context.PropertyTrashEntries
            .Where(x => x.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(x => x.DeletedDate)
            .AsNoTracking()
            .Select(x => new PropertyTrashEntry
            {
                Id = x.Id,
                PropertyId = x.PropertyId,
                PropertyName = x.PropertyName,
                PropertyCode = x.PropertyCode,
                CatalogId = x.CatalogId,
                CategoryId = x.CategoryId,
                PropertyDataJson = x.PropertyDataJson,
                DeletedDate = x.DeletedDate,
                DeletedBy = x.DeletedBy,
                ExpirationDate = x.ExpirationDate,
            })
            .ToListAsync();
    }

    public async Task MoveToTrashAsync(string propertyId, string deletedBy)
    {
        var properties = await _propertyService.GetByIdsAsync(new[] { propertyId });
        var property = properties.FirstOrDefault();

        if (property == null)
        {
            return;
        }

        var trashEntry = new PropertyTrashEntity
        {
            Id = Guid.NewGuid().ToString(),
            PropertyId = propertyId,
            PropertyName = property.Name,
            PropertyCode = property.Name,
            CatalogId = property.CatalogId,
            CategoryId = property.CategoryId,
            PropertyDataJson = JsonSerializer.Serialize(property),
            DeletedDate = DateTime.UtcNow,
            DeletedBy = string.IsNullOrWhiteSpace(deletedBy) ? "system" : deletedBy,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
        };

        await using (var context = _dbContextFactory())
        {
            context.PropertyTrashEntries.Add(trashEntry);
            await context.SaveChangesAsync();
        }

        await _propertyService.DeleteAsync(new[] { propertyId });
    }

    public async Task RestoreFromTrashAsync(string trashEntryId)
    {
        PropertyTrashEntity entry;

        await using var context = _dbContextFactory();
        entry = await context.PropertyTrashEntries.FirstOrDefaultAsync(x => x.Id == trashEntryId);

        if (entry == null)
        {
            return;
        }

        var property = JsonSerializer.Deserialize<Property>(entry.PropertyDataJson);
        if (property != null)
        {
            property.Id = null;
            await _propertyService.SaveChangesAsync(new[] { property });
        }

        context.PropertyTrashEntries.Remove(entry);
        await context.SaveChangesAsync();
    }

    public async Task DeletePermanentlyAsync(string propertyId)
    {
        await _propertyService.DeleteAsync(new[] { propertyId });
    }

    public Task CleanupExpiredAsync()
    {
        return CleanupExpiredInternalAsync();
    }

    private async Task CleanupExpiredInternalAsync()
    {
        await using var context = _dbContextFactory();
        var expiredEntries = await context.PropertyTrashEntries
            .Where(entry => entry.ExpirationDate <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredEntries.Count == 0)
        {
            return;
        }

        context.PropertyTrashEntries.RemoveRange(expiredEntries);
        await context.SaveChangesAsync();
    }
}

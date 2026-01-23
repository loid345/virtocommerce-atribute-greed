using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.AttributeGrid.Data.Services;

public class PropertyTrashService : IPropertyTrashService
{
    private readonly IPropertyService _propertyService;
    private static readonly List<PropertyTrashEntry> TrashEntries = new();
    private static readonly object TrashLock = new();

    public PropertyTrashService(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public Task<IList<PropertyTrashEntry>> GetTrashEntriesAsync()
    {
        List<PropertyTrashEntry> result;

        lock (TrashLock)
        {
            result = TrashEntries
                .Where(x => x.ExpirationDate > DateTime.UtcNow)
                .OrderByDescending(x => x.DeletedDate)
                .ToList();
        }

        return Task.FromResult<IList<PropertyTrashEntry>>(result);
    }

    public async Task MoveToTrashAsync(string propertyId, string deletedBy)
    {
        var properties = await _propertyService.GetByIdsAsync(new[] { propertyId });
        var property = properties.FirstOrDefault();

        if (property == null)
        {
            return;
        }

        var trashEntry = new PropertyTrashEntry
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

        lock (TrashLock)
        {
            TrashEntries.Add(trashEntry);
        }

        await _propertyService.DeleteAsync(new[] { propertyId });
    }

    public async Task RestoreFromTrashAsync(string trashEntryId)
    {
        PropertyTrashEntry entry;

        lock (TrashLock)
        {
            entry = TrashEntries.FirstOrDefault(x => x.Id == trashEntryId);
        }

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

        lock (TrashLock)
        {
            TrashEntries.Remove(entry);
        }
    }

    public async Task DeletePermanentlyAsync(string propertyId)
    {
        await _propertyService.DeleteAsync(new[] { propertyId });
    }

    public Task CleanupExpiredAsync()
    {
        lock (TrashLock)
        {
            TrashEntries.RemoveAll(entry => entry.ExpirationDate <= DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }
}

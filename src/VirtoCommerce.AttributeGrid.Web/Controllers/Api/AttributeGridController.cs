using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.AttributeGrid.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.AttributeGrid.Web.Controllers.Api;

[Authorize]
[Route("api/attribute-grid")]
public class AttributeGridController : Controller
{
    private readonly IPropertyManagerService _propertyManagerService;
    private readonly IPropertyTrashService _propertyTrashService;
    private readonly IPropertySearchService _propertySearchService;
    private readonly ICatalogService _catalogService;
    private readonly ICategoryService _categoryService;

    public AttributeGridController(
        IPropertyManagerService propertyManagerService,
        IPropertyTrashService propertyTrashService,
        IPropertySearchService propertySearchService,
        ICatalogService catalogService,
        ICategoryService categoryService)
    {
        _propertyManagerService = propertyManagerService;
        _propertyTrashService = propertyTrashService;
        _propertySearchService = propertySearchService;
        _catalogService = catalogService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Search properties with filtering and pagination.
    /// </summary>
    [HttpPost]
    [Route("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<GenericSearchResult<PropertyListItem>>> SearchProperties([FromBody] PropertySearchCriteria criteria)
    {
        criteria ??= new PropertySearchCriteria();

        var requiresInMemoryFiltering = !string.IsNullOrEmpty(criteria.CategoryId)
            || !string.IsNullOrEmpty(criteria.ValueType)
            || !string.IsNullOrEmpty(criteria.PropertyType)
            || criteria.IsFilterable.HasValue
            || criteria.IsDictionary.HasValue;

        var searchCriteria = new CatalogPropertySearchCriteria
        {
            Keyword = criteria.Keyword,
            CatalogIds = string.IsNullOrEmpty(criteria.CatalogId)
                ? null
                : new[] { criteria.CatalogId },
            Skip = requiresInMemoryFiltering ? 0 : criteria.Skip,
            Take = requiresInMemoryFiltering ? int.MaxValue : criteria.Take,
            Sort = criteria.Sort,
        };

        var searchResult = await _propertySearchService.SearchPropertiesAsync(searchCriteria);
        var filteredProperties = new List<Property>();

        foreach (var property in searchResult.Results)
        {
            if (!string.IsNullOrEmpty(criteria.CategoryId)
                && !string.Equals(property.CategoryId, criteria.CategoryId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(criteria.ValueType)
                && !string.Equals(property.ValueType.ToString(), criteria.ValueType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(criteria.PropertyType)
                && !string.Equals(property.Type.ToString(), criteria.PropertyType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (criteria.IsFilterable.HasValue && property.IsFilterable != criteria.IsFilterable.Value)
            {
                continue;
            }

            if (criteria.IsDictionary.HasValue && property.Dictionary != criteria.IsDictionary.Value)
            {
                continue;
            }

            filteredProperties.Add(property);
        }

        var totalCount = filteredProperties.Count;
        var pageProperties = requiresInMemoryFiltering
            ? filteredProperties.Skip(criteria.Skip).Take(criteria.Take).ToList()
            : filteredProperties;

        var catalogIds = pageProperties.Select(x => x.CatalogId)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToArray();
        var categoryIds = pageProperties.Select(x => x.CategoryId)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToArray();

        var catalogs = catalogIds.Length == 0
            ? Array.Empty<Catalog>()
            : await _catalogService.GetByIdsAsync(catalogIds, CatalogResponseGroup.Info.ToString());
        var categories = categoryIds.Length == 0
            ? Array.Empty<Category>()
            : await _categoryService.GetByIdsAsync(categoryIds, CategoryResponseGroup.Info.ToString());

        var catalogMap = catalogs.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
        var categoryMap = categories.ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);

        var items = pageProperties.Select(property =>
        {
            var ownerPath = BuildOwnerPath(property, catalogMap, categoryMap);

            return new PropertyListItem
            {
                Id = property.Id,
                Name = property.Name,
                Code = property.Name,
                ValueType = property.ValueType.ToString(),
                PropertyType = property.Type.ToString(),
                CatalogId = property.CatalogId,
                CatalogName = GetCatalogName(property.CatalogId, catalogMap),
                CategoryId = property.CategoryId,
                OwnerPath = ownerPath,
                IsFilterable = property.IsFilterable,
                IsDictionary = property.Dictionary,
                IsRequired = property.IsRequired,
                IsMultivalue = property.Multivalue,
                UsageCount = 0,
            };
        }).ToList();

        var result = new GenericSearchResult<PropertyListItem>
        {
            TotalCount = requiresInMemoryFiltering ? totalCount : searchResult.TotalCount,
            Results = items,
        };

        return Ok(result);
    }

    /// <summary>
    /// Get property by id.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<PropertyListItem>> GetProperty(string id)
    {
        var result = await _propertyManagerService.GetPropertyByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update property flags.
    /// </summary>
    [HttpPatch]
    [Route("{id}")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> UpdateProperty(string id, [FromBody] PropertyUpdateRequest request)
    {
        await _propertyManagerService.UpdatePropertyAsync(id, request.IsFilterable, request.IsRequired);
        return NoContent();
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> DeleteProperty(string id)
    {
        await _propertyTrashService.MoveToTrashAsync(id, User?.Identity?.Name);
        return NoContent();
    }

    [HttpGet]
    [Route("trash")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<IList<PropertyTrashEntry>>> GetTrashEntries()
    {
        var entries = await _propertyTrashService.GetTrashEntriesAsync();
        return Ok(entries);
    }

    [HttpPost]
    [Route("trash/{id}/restore")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> RestoreTrashEntry(string id)
    {
        await _propertyTrashService.RestoreFromTrashAsync(id);
        return NoContent();
    }

    private static string BuildOwnerPath(
        Property property,
        IReadOnlyDictionary<string, string> catalogMap,
        IReadOnlyDictionary<string, Category> categoryMap)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(property.CatalogId)
            && catalogMap.TryGetValue(property.CatalogId, out var catalogName))
        {
            parts.Add(catalogName);
        }

        if (!string.IsNullOrEmpty(property.CategoryId)
            && categoryMap.TryGetValue(property.CategoryId, out var category))
        {
            parts.Add(category.Name);
        }

        return parts.Count == 0 ? "Global" : string.Join(" / ", parts);
    }

    private static string GetCatalogName(string catalogId, IReadOnlyDictionary<string, string> catalogMap)
    {
        if (string.IsNullOrEmpty(catalogId))
        {
            return null;
        }

        return catalogMap.TryGetValue(catalogId, out var catalogName) ? catalogName : null;
    }
}

public class PropertyUpdateRequest
{
    public bool? IsFilterable { get; set; }
    public bool? IsRequired { get; set; }
}

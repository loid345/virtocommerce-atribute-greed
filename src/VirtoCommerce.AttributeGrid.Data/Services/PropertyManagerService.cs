using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.AttributeGrid.Core;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

using CatalogPropertySearchCriteria = VirtoCommerce.CatalogModule.Core.Model.Search.PropertySearchCriteria;

namespace VirtoCommerce.AttributeGrid.Data.Services;

public class PropertyManagerService : IPropertyManagerService
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertySearchService _propertySearchService;
    private readonly ICatalogService _catalogService;
    private readonly ICategoryService _categoryService;

    public PropertyManagerService(
        IPropertyService propertyService,
        IPropertySearchService propertySearchService,
        ICatalogService catalogService,
        ICategoryService categoryService)
    {
        _propertyService = propertyService;
        _propertySearchService = propertySearchService;
        _catalogService = catalogService;
        _categoryService = categoryService;
    }

    public async Task<GenericSearchResult<PropertyListItem>> SearchPropertiesAsync(PropertySearchCriteria criteria)
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
        var items = new List<PropertyListItem>();

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

            if (criteria.IsFilterable.HasValue && CatalogModuleHelper.GetIsFilterable(property) != criteria.IsFilterable.Value)
            {
                continue;
            }

            if (criteria.IsDictionary.HasValue && property.Dictionary != criteria.IsDictionary.Value)
            {
                continue;
            }

            var item = await MapToListItemAsync(property);
            items.Add(item);
        }

        var totalCount = items.Count;
        if (requiresInMemoryFiltering)
        {
            items = items
                .Skip(criteria.Skip)
                .Take(criteria.Take)
                .ToList();
        }

        return new GenericSearchResult<PropertyListItem>
        {
            TotalCount = requiresInMemoryFiltering ? totalCount : searchResult.TotalCount,
            Results = items,
        };
    }

    public async Task<PropertyListItem> GetPropertyByIdAsync(string propertyId)
    {
        var properties = await _propertyService.GetByIdsAsync(new[] { propertyId });
        var property = properties.FirstOrDefault();

        return property == null ? null : await MapToListItemAsync(property);
    }

    public async Task UpdatePropertyAsync(string propertyId, bool? isFilterable = null, bool? isRequired = null)
    {
        var properties = await _propertyService.GetByIdsAsync(new[] { propertyId });
        var property = properties.FirstOrDefault();

        if (property == null)
        {
            return;
        }

        if (isFilterable.HasValue)
        {
            CatalogModuleHelper.SetIsFilterable(property, isFilterable.Value);
        }

        if (isRequired.HasValue)
        {
            CatalogModuleHelper.SetIsRequired(property, isRequired.Value);
        }

        await _propertyService.SaveChangesAsync(new[] { property });
    }

    private async Task<PropertyListItem> MapToListItemAsync(Property property)
    {
        var item = new PropertyListItem
        {
            Id = property.Id,
            Name = property.Name,
            Code = CatalogModuleHelper.GetCode(property) ?? property.Name,
            ValueType = property.ValueType.ToString(),
            PropertyType = property.Type.ToString(),
            CatalogId = property.CatalogId,
            CategoryId = property.CategoryId,
            IsFilterable = CatalogModuleHelper.GetIsFilterable(property),
            IsDictionary = property.Dictionary,
            IsRequired = CatalogModuleHelper.GetIsRequired(property),
            IsMultivalue = property.Multivalue,
            UsageCount = 0,
        };

        var ownerInfo = await BuildOwnerPathAsync(property);
        item.OwnerPath = ownerInfo.OwnerPath;
        item.CatalogName = ownerInfo.CatalogName;

        return item;
    }

    private async Task<(string OwnerPath, string CatalogName)> BuildOwnerPathAsync(Property property)
    {
        var parts = new List<string>();
        string catalogName = null;

        if (!string.IsNullOrEmpty(property.CatalogId))
        {
            var catalogs = await CatalogModuleHelper.GetCatalogsAsync(
                _catalogService,
                new[] { property.CatalogId },
                CatalogResponseGroup.Info.ToString());

            var catalog = catalogs.FirstOrDefault();
            if (catalog != null)
            {
                parts.Add(catalog.Name);
                catalogName = catalog.Name;
            }
        }

        if (!string.IsNullOrEmpty(property.CategoryId))
        {
            var categories = await CatalogModuleHelper.GetCategoriesAsync(
                _categoryService,
                new[] { property.CategoryId },
                property.CatalogId,
                CategoryResponseGroup.Info.ToString());

            var category = categories.FirstOrDefault();
            if (category != null)
            {
                parts.Add(category.Name);
            }
        }

        return (string.Join(" / ", parts), catalogName);
    }
}

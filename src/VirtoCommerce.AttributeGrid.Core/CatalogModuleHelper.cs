using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.AttributeGrid.Core;

public static class CatalogModuleHelper
{
    public static bool GetIsFilterable(Property property)
    {
        return GetBooleanProperty(property, "IsFilterable", "Filterable");
    }

    public static void SetIsFilterable(Property property, bool value)
    {
        SetBooleanProperty(property, value, "IsFilterable", "Filterable");
    }

    public static bool GetIsRequired(Property property)
    {
        return GetBooleanProperty(property, "IsRequired", "Required");
    }

    public static void SetIsRequired(Property property, bool value)
    {
        SetBooleanProperty(property, value, "IsRequired", "Required");
    }

    public static string GetCode(Property property)
    {
        return GetStringProperty(property, "Code");
    }

    public static async Task<IList<Catalog>> GetCatalogsAsync(
        ICatalogService catalogService,
        IList<string> catalogIds,
        string responseGroup)
    {
        if (catalogIds == null || catalogIds.Count == 0)
        {
            return Array.Empty<Catalog>();
        }

        var serviceType = catalogService.GetType();
        var getByIds = serviceType.GetMethod("GetByIdsAsync", new[] { typeof(IList<string>), typeof(string) });
        if (getByIds != null)
        {
            return await InvokeAsync<IList<Catalog>>(catalogService, getByIds, new object[] { catalogIds, responseGroup });
        }

        var getByIdWithGroup = serviceType.GetMethod("GetByIdAsync", new[] { typeof(string), typeof(string) });
        if (getByIdWithGroup != null)
        {
            var results = new List<Catalog>();
            foreach (var id in catalogIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                var catalog = await InvokeAsync<Catalog>(catalogService, getByIdWithGroup, new object[] { id, responseGroup });
                if (catalog != null)
                {
                    results.Add(catalog);
                }
            }

            return results;
        }

        var getById = serviceType.GetMethod("GetByIdAsync", new[] { typeof(string) });
        if (getById != null)
        {
            var results = new List<Catalog>();
            foreach (var id in catalogIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                var catalog = await InvokeAsync<Catalog>(catalogService, getById, new object[] { id });
                if (catalog != null)
                {
                    results.Add(catalog);
                }
            }

            return results;
        }

        return Array.Empty<Catalog>();
    }

    public static async Task<IList<Category>> GetCategoriesAsync(
        ICategoryService categoryService,
        IList<string> categoryIds,
        string catalogId,
        string responseGroup)
    {
        if (categoryIds == null || categoryIds.Count == 0)
        {
            return Array.Empty<Category>();
        }

        var serviceType = categoryService.GetType();
        var getByIdsWithCatalog = serviceType.GetMethod(
            "GetByIdsAsync",
            new[] { typeof(IList<string>), typeof(string), typeof(string) });
        if (getByIdsWithCatalog != null)
        {
            return await InvokeAsync<IList<Category>>(
                categoryService,
                getByIdsWithCatalog,
                new object[] { categoryIds, catalogId, responseGroup });
        }

        var getByIds = serviceType.GetMethod("GetByIdsAsync", new[] { typeof(IList<string>), typeof(string) });
        if (getByIds != null)
        {
            return await InvokeAsync<IList<Category>>(categoryService, getByIds, new object[] { categoryIds, responseGroup });
        }

        var getByIdsNoGroup = serviceType.GetMethod("GetByIdsAsync", new[] { typeof(IList<string>) });
        if (getByIdsNoGroup != null)
        {
            return await InvokeAsync<IList<Category>>(categoryService, getByIdsNoGroup, new object[] { categoryIds });
        }

        var getByIdWithCatalog = serviceType.GetMethod(
            "GetByIdAsync",
            new[] { typeof(string), typeof(string), typeof(string) });
        if (getByIdWithCatalog != null)
        {
            var results = new List<Category>();
            foreach (var id in categoryIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                var category = await InvokeAsync<Category>(
                    categoryService,
                    getByIdWithCatalog,
                    new object[] { id, catalogId, responseGroup });
                if (category != null)
                {
                    results.Add(category);
                }
            }

            return results;
        }

        var getByIdWithGroup = serviceType.GetMethod("GetByIdAsync", new[] { typeof(string), typeof(string) });
        if (getByIdWithGroup != null)
        {
            var results = new List<Category>();
            foreach (var id in categoryIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                var category = await InvokeAsync<Category>(
                    categoryService,
                    getByIdWithGroup,
                    new object[] { id, responseGroup });
                if (category != null)
                {
                    results.Add(category);
                }
            }

            return results;
        }

        var getById = serviceType.GetMethod("GetByIdAsync", new[] { typeof(string) });
        if (getById != null)
        {
            var results = new List<Category>();
            foreach (var id in categoryIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                var category = await InvokeAsync<Category>(categoryService, getById, new object[] { id });
                if (category != null)
                {
                    results.Add(category);
                }
            }

            return results;
        }

        return Array.Empty<Category>();
    }

    public static async Task SavePropertyAsync(IPropertyService propertyService, Property property, bool isNew)
    {
        if (propertyService == null || property == null)
        {
            return;
        }

        var serviceType = propertyService.GetType();
        var payload = new[] { property };

        if (isNew)
        {
            var createWithList = serviceType.GetMethod("CreateAsync", new[] { typeof(IList<Property>) });
            if (createWithList != null)
            {
                await InvokeAsync<object>(propertyService, createWithList, new object[] { payload });
                return;
            }

            var createWithEnumerable = serviceType.GetMethod("CreateAsync", new[] { typeof(IEnumerable<Property>) });
            if (createWithEnumerable != null)
            {
                await InvokeAsync<object>(propertyService, createWithEnumerable, new object[] { payload });
                return;
            }
        }

        var saveWithList = serviceType.GetMethod("SaveChangesAsync", new[] { typeof(IList<Property>) });
        if (saveWithList != null)
        {
            await InvokeAsync<object>(propertyService, saveWithList, new object[] { payload });
            return;
        }

        var saveWithEnumerable = serviceType.GetMethod("SaveChangesAsync", new[] { typeof(IEnumerable<Property>) });
        if (saveWithEnumerable != null)
        {
            await InvokeAsync<object>(propertyService, saveWithEnumerable, new object[] { payload });
        }
    }

    private static bool GetBooleanProperty(object target, params string[] propertyNames)
    {
        var propertyInfo = GetPropertyInfo(target, propertyNames);
        if (propertyInfo == null)
        {
            return default;
        }

        var value = propertyInfo.GetValue(target);
        return value is bool boolValue && boolValue;
    }

    private static void SetBooleanProperty(object target, bool value, params string[] propertyNames)
    {
        var propertyInfo = GetPropertyInfo(target, propertyNames);
        if (propertyInfo == null)
        {
            return;
        }

        propertyInfo.SetValue(target, value);
    }

    private static string GetStringProperty(object target, params string[] propertyNames)
    {
        var propertyInfo = GetPropertyInfo(target, propertyNames);
        if (propertyInfo == null)
        {
            return null;
        }

        return propertyInfo.GetValue(target) as string;
    }

    private static PropertyInfo GetPropertyInfo(object target, params string[] propertyNames)
    {
        if (target == null)
        {
            return null;
        }

        var targetType = target.GetType();
        return propertyNames
            .Select(name => targetType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance))
            .FirstOrDefault(property => property != null);
    }

    private static async Task<T> InvokeAsync<T>(object target, MethodInfo method, object[] parameters)
    {
        var task = (Task)method.Invoke(target, parameters);
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty == null ? default : (T)resultProperty.GetValue(task);
    }
}

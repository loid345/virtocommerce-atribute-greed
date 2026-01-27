using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AttributeGrid.Core;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
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
    private readonly IPropertyExportService _propertyExportService;
    private readonly IPropertyService _propertyService;

    public AttributeGridController(
        IPropertyManagerService propertyManagerService,
        IPropertyTrashService propertyTrashService,
        IPropertyExportService propertyExportService,
        IPropertyService propertyService)
    {
        _propertyManagerService = propertyManagerService;
        _propertyTrashService = propertyTrashService;
        _propertyExportService = propertyExportService;
        _propertyService = propertyService;
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
        var result = await _propertyManagerService.SearchPropertiesAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Export properties to Excel.
    /// </summary>
    [HttpGet]
    [Route("export")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult> Export([FromQuery] PropertySearchCriteria criteria)
    {
        criteria ??= new PropertySearchCriteria();
        criteria.Skip = 0;
        criteria.Take = 10000;

        var searchResult = await _propertyManagerService.SearchPropertiesAsync(criteria);
        var bytes = _propertyExportService.ExportToExcel(searchResult.Results ?? Array.Empty<PropertyListItem>());

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "attribute-grid-export.xlsx");
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

    [HttpPost]
    [Route("trash")]
    [Authorize(Permissions.Delete)]
    public async Task<ActionResult> MoveToTrash([FromBody] string[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return NoContent();
        }

        var username = User?.Identity?.Name ?? "admin";

        foreach (var id in ids.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            await _propertyTrashService.MoveToTrashAsync(id, username);
        }

        return NoContent();
    }

    [HttpPut]
    [Route("")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<Property>> Update([FromBody] PropertyListItem item)
    {
        if (item == null)
        {
            return BadRequest();
        }

        var property = string.IsNullOrEmpty(item.Id)
            ? new Property()
            : (await _propertyService.GetByIdsAsync(new[] { item.Id })).FirstOrDefault() ?? new Property();

        if (!string.IsNullOrEmpty(item.Id))
        {
            property.Id = item.Id;
        }

        if (!string.IsNullOrEmpty(item.Name))
        {
            property.Name = item.Name;
        }

        if (string.IsNullOrWhiteSpace(property.Name))
        {
            return BadRequest("Property name is required.");
        }

        if (!string.IsNullOrEmpty(item.CatalogId))
        {
            property.CatalogId = item.CatalogId;
        }

        if (!string.IsNullOrEmpty(item.CategoryId))
        {
            property.CategoryId = item.CategoryId;
        }

        if (!string.IsNullOrEmpty(item.ValueType)
            && Enum.TryParse<PropertyValueType>(item.ValueType, true, out var valueType))
        {
            property.ValueType = valueType;
        }

        if (!string.IsNullOrEmpty(item.PropertyType)
            && Enum.TryParse<PropertyType>(item.PropertyType, true, out var propertyType))
        {
            property.Type = propertyType;
        }

        CatalogModuleHelper.SetIsFilterable(property, item.IsFilterable);
        CatalogModuleHelper.SetIsRequired(property, item.IsRequired);
        property.Multivalue = item.IsMultivalue;
        property.Dictionary = item.IsDictionary;

        if (string.IsNullOrEmpty(item.Id))
        {
            if (string.IsNullOrEmpty(property.Id))
            {
                property.Id = Guid.NewGuid().ToString();
            }

            await CatalogModuleHelper.SavePropertyAsync(_propertyService, property, true);
        }
        else
        {
            await CatalogModuleHelper.SavePropertyAsync(_propertyService, property, false);
        }

        return Ok(property);
    }

    /// <summary>
    /// Bulk update property flags.
    /// </summary>
    [HttpPost]
    [Route("bulk/update")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> BulkUpdate([FromBody] BulkUpdateRequest request)
    {
        if (request?.Ids == null || request.Ids.Count == 0)
        {
            return Ok(new { updated = 0 });
        }

        foreach (var id in request.Ids)
        {
            await _propertyManagerService.UpdatePropertyAsync(id, request.IsFilterable, request.IsRequired);
        }

        return Ok(new { updated = request.Ids.Count });
    }

    /// <summary>
    /// Bulk update property flags (legacy payload).
    /// </summary>
    [HttpPost]
    [Route("bulk-update")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult> BulkUpdate([FromBody] BulkPropertyUpdateRequest request)
    {
        if (request?.PropertyIds == null || request.PropertyIds.Length == 0)
        {
            return Ok(new { updated = 0 });
        }

        foreach (var id in request.PropertyIds)
        {
            await _propertyManagerService.UpdatePropertyAsync(id, request.IsFilterable, request.IsRequired);
        }

        return Ok(new { updated = request.PropertyIds.Length });
    }

    /// <summary>
    /// Bulk move properties to trash.
    /// </summary>
    [HttpPost]
    [Route("bulk/delete")]
    [Authorize(Permissions.Delete)]
    public async Task<ActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        if (request?.Ids == null || request.Ids.Count == 0)
        {
            return Ok(new { deleted = 0 });
        }

        foreach (var id in request.Ids)
        {
            await _propertyTrashService.MoveToTrashAsync(id, User?.Identity?.Name);
        }

        return Ok(new { deleted = request.Ids.Count });
    }

    /// <summary>
    /// Bulk move properties to trash (legacy payload).
    /// </summary>
    [HttpPost]
    [Route("bulk-delete")]
    [Authorize(Permissions.Delete)]
    public async Task<ActionResult> BulkDelete([FromBody] BulkPropertyDeleteRequest request)
    {
        if (request?.PropertyIds == null || request.PropertyIds.Length == 0)
        {
            return Ok(new { deleted = 0 });
        }

        foreach (var id in request.PropertyIds)
        {
            await _propertyTrashService.MoveToTrashAsync(id, User?.Identity?.Name);
        }

        return Ok(new { deleted = request.PropertyIds.Length });
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

}

public class PropertyUpdateRequest
{
    public bool? IsFilterable { get; set; }
    public bool? IsRequired { get; set; }
}

public class BulkUpdateRequest
{
    public List<string> Ids { get; set; } = new();
    public bool? IsFilterable { get; set; }
    public bool? IsRequired { get; set; }
}

public class BulkDeleteRequest
{
    public List<string> Ids { get; set; } = new();
}

public class BulkPropertyUpdateRequest
{
    public string[] PropertyIds { get; set; }
    public bool? IsFilterable { get; set; }
    public bool? IsRequired { get; set; }
}

public class BulkPropertyDeleteRequest
{
    public string[] PropertyIds { get; set; }
}

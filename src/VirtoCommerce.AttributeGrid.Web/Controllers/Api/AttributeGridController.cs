using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.AttributeGrid.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.AttributeGrid.Web.Controllers.Api;

[Authorize]
[Route("api/attribute-grid")]
public class AttributeGridController : Controller
{
    private readonly IPropertyManagerService _propertyManagerService;

    public AttributeGridController(IPropertyManagerService propertyManagerService)
    {
        _propertyManagerService = propertyManagerService;
    }

    /// <summary>
    /// Search properties with filtering and pagination.
    /// </summary>
    [HttpPost]
    [Route("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<GenericSearchResult<PropertyListItem>>> SearchProperties([FromBody] PropertySearchCriteria criteria)
    {
        var result = await _propertyManagerService.SearchPropertiesAsync(criteria);
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
}

public class PropertyUpdateRequest
{
    public bool? IsFilterable { get; set; }
    public bool? IsRequired { get; set; }
}

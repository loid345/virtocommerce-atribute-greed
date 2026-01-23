using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Permissions = VirtoCommerce.AttributeGrid.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.AttributeGrid.Web.Controllers.Api;

[Authorize]
[Route("api/attribute-grid")]
public class AttributeGridController : Controller
{
    // GET: api/attribute-grid
    /// <summary>
    /// Get sample response
    /// </summary>
    /// <remarks>Return sample data for Attribute Grid module</remarks>
    [HttpGet]
    [Route("")]
    [Authorize(Permissions.Read)]
    public ActionResult<string> Get()
    {
        return Ok(new { result = "Attribute Grid module is running." });
    }
}

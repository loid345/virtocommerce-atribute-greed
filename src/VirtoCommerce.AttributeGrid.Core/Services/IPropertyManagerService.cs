using System.Threading.Tasks;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.AttributeGrid.Core.Services;

public interface IPropertyManagerService
{
    Task<GenericSearchResult<PropertyListItem>> SearchPropertiesAsync(PropertySearchCriteria criteria);
    Task<PropertyListItem> GetPropertyByIdAsync(string propertyId);
    Task UpdatePropertyAsync(string propertyId, bool? isFilterable = null, bool? isRequired = null);
}

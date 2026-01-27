using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.AttributeGrid.Core.Models;

public class PropertySearchCriteria : SearchCriteriaBase
{
    public string CatalogId { get; set; }
    public string CategoryId { get; set; }
    public string ValueType { get; set; }
    public string PropertyType { get; set; }
    public bool? IsFilterable { get; set; }
    public bool? IsDictionary { get; set; }
}

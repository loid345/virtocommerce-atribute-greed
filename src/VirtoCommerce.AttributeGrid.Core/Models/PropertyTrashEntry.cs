using System;

namespace VirtoCommerce.AttributeGrid.Core.Models;

public class PropertyTrashEntry
{
    public string Id { get; set; }
    public string PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string PropertyCode { get; set; }
    public string CatalogId { get; set; }
    public string CategoryId { get; set; }
    public string PropertyDataJson { get; set; }
    public DateTime DeletedDate { get; set; }
    public string DeletedBy { get; set; }
    public DateTime ExpirationDate { get; set; }
}

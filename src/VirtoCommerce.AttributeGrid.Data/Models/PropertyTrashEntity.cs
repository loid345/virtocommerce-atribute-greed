using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.AttributeGrid.Data.Models;

public class PropertyTrashEntity : Entity
{
    public string PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string PropertyCode { get; set; }
    public string CatalogId { get; set; }
    public string CategoryId { get; set; }
    public string PropertyDataJson { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}

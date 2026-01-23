namespace VirtoCommerce.AttributeGrid.Core.Models;

/// <summary>
/// Lightweight model for attribute grid list.
/// </summary>
public class PropertyListItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string ValueType { get; set; }
    public string PropertyType { get; set; }

    public string CatalogId { get; set; }
    public string CatalogName { get; set; }
    public string CategoryId { get; set; }
    public string OwnerPath { get; set; }

    public bool IsFilterable { get; set; }
    public bool IsDictionary { get; set; }
    public bool IsRequired { get; set; }
    public bool IsMultivalue { get; set; }

    public int UsageCount { get; set; }
}

using System.Collections.Generic;
using VirtoCommerce.AttributeGrid.Core.Models;

namespace VirtoCommerce.AttributeGrid.Core.Services;

public interface IPropertyExportService
{
    byte[] ExportToExcel(IEnumerable<PropertyListItem> items);
}

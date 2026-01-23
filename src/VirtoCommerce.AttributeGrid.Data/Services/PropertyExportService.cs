using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using VirtoCommerce.AttributeGrid.Core.Models;
using VirtoCommerce.AttributeGrid.Core.Services;

namespace VirtoCommerce.AttributeGrid.Data.Services;

public class PropertyExportService : IPropertyExportService
{
    public byte[] ExportToExcel(IEnumerable<PropertyListItem> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attributes");

        worksheet.Cell(1, 1).Value = "Name";
        worksheet.Cell(1, 2).Value = "Code";
        worksheet.Cell(1, 3).Value = "Type";
        worksheet.Cell(1, 4).Value = "Catalog";
        worksheet.Cell(1, 5).Value = "Category";
        worksheet.Cell(1, 6).Value = "IsFilterable";
        worksheet.Cell(1, 7).Value = "IsRequired";
        worksheet.Cell(1, 8).Value = "IsMultiValue";
        worksheet.Cell(1, 9).Value = "UsageCount";

        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Name;
            worksheet.Cell(row, 2).Value = item.Code;
            worksheet.Cell(row, 3).Value = item.ValueType;
            worksheet.Cell(row, 4).Value = string.IsNullOrEmpty(item.CatalogName) ? item.CatalogId : item.CatalogName;
            worksheet.Cell(row, 5).Value = string.IsNullOrEmpty(item.OwnerPath) ? item.CategoryId : item.OwnerPath;
            worksheet.Cell(row, 6).Value = item.IsFilterable ? "Yes" : "No";
            worksheet.Cell(row, 7).Value = item.IsRequired ? "Yes" : "No";
            worksheet.Cell(row, 8).Value = item.IsMultivalue ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = item.UsageCount;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

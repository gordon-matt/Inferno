using System.Data;
using Extenso;
using OfficeOpenXml;

namespace Inferno;

/// <summary>Writes a <see cref="DataTable"/> to an Excel workbook.</summary>
public static class DataTableExtensions
{
    /// <summary>Writes the table to an .xlsx workbook, including column names.</summary>
    /// <param name="table">The table to write. Must not be <see langword="null"/>.</param>
    /// <returns>The workbook bytes.</returns>
    public static byte[] ToXlsx(this DataTable table) => table.ToXlsx(true);

    /// <summary>
    /// Writes the table to one worksheet and returns the workbook bytes.
    /// The sheet name is <see cref="DataTable.TableName"/>, or "Report" when that is empty.
    /// Date columns use the format <c>yyyy-mm-dd hh:mm</c>. A text column wraps when any value contains a newline.
    /// </summary>
    /// <param name="table">The table to write. Must not be <see langword="null"/>.</param>
    /// <param name="outputColumnNames"><see langword="true"/> to write the column names as the first row.</param>
    /// <returns>The workbook bytes.</returns>
    public static byte[] ToXlsx(this DataTable table, bool outputColumnNames)
    {
        using var excel = new ExcelPackage();

        string sheetName = string.IsNullOrEmpty(table.TableName)
            ? "Report"
            : table.TableName;

        using var worksheet = excel.Workbook.Worksheets.Add(sheetName);
        worksheet.Cells["A1"].LoadFromDataTable(table, outputColumnNames);

        int columnNumber = 1;
        foreach (DataColumn column in table.Columns)
        {
            if (column.DataType == typeof(DateTime))
            {
                worksheet.Column(columnNumber).Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
            }
            else if (column.DataType == typeof(string))
            {
                if (table.Rows.OfType<DataRow>().Any(x => !x.IsNull(column) && x.Field<string>(column).ContainsAny("\n", "\r\n")))
                {
                    worksheet.Column(columnNumber).Style.WrapText = true;
                }
            }
            columnNumber++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return excel.GetAsByteArray();
    }
}

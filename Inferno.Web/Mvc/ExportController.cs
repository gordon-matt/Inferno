using System.Data;
using System.Text;
using Extenso;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Mvc;

/// <summary>Writes an entity query as a delimited file or an Excel workbook.</summary>
/// <typeparam name="T">The entity type. Its name is used in the file name.</typeparam>
public partial class ExportController<T> : InfernoController
     where T : class, IEntity
{
    /// <summary>
    /// Writes <paramref name="query"/> in the format <paramref name="options"/> names.
    /// The file name is the entity type and the local time.
    /// </summary>
    /// <param name="query">The rows to export.</param>
    /// <param name="options">The format and, for a delimited file, the separator and quoting.</param>
    /// <returns>The file.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="DownloadOptions.FileFormat"/> is not a known format.</exception>
    [NonAction]
    public FileResult Download(IQueryable<T> query, DownloadOptions options)
    {
        switch (options.FileFormat)
        {
            case DownloadFileFormat.Delimited:
                {
                    string separator;
                    string contentType;
                    string fileExtension;

                    switch (options.Delimiter)
                    {
                        case DownloadFileDelimiter.Tab:
                            separator = "\t";
                            contentType = "text/tab-separated-values";
                            fileExtension = "tsv";
                            break;

                        case DownloadFileDelimiter.VerticalBar:
                            separator = "|";
                            contentType = "text/plain";
                            fileExtension = "txt";
                            break;

                        case DownloadFileDelimiter.Semicolon:
                            separator = ";";
                            contentType = "text/plain";
                            fileExtension = "txt";
                            break;

                        case DownloadFileDelimiter.Comma:
                        default:
                            separator = ",";
                            contentType = "text/csv";
                            fileExtension = "csv";
                            break;
                    }

                    string delimited = query.ToDelimited(
                        delimiter: separator,
                        outputColumnNames: options.OutputColumnNames,
                        alwaysEnquote: options.AlwaysEnquote);

                    return File(Encoding.UTF8.GetBytes(delimited), contentType, $"{query.ElementType}_{DateTime.Now:yyyy-MM-dd HH_mm_ss}.{fileExtension}");
                }
            case DownloadFileFormat.XLSX:
                {
                    byte[] bytes = query.ToDataTable().ToXlsx();
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{query.ElementType}_{DateTime.Now:yyyy-MM-dd HH_mm_ss}.xlsx");
                }
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>Writes <paramref name="data"/> as UTF-8 JSON. <c>.json</c> is appended when <paramref name="fileName"/> does not end with it.</summary>
    /// <typeparam name="TData">The value being serialized.</typeparam>
    /// <param name="data">The value to serialize.</param>
    /// <param name="fileName">The download name.</param>
    /// <returns>The JSON file.</returns>
    [NonAction]
    public FileResult DownloadJson<TData>(TData data, string fileName)
    {
        if (!fileName.EndsWith(".json"))
        {
            fileName += ".json";
        }

        string json = data.JsonSerialize();
        return File(new UTF8Encoding().GetBytes(json), "application/json", fileName);
    }
}
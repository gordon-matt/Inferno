using System.Data;
using System.Text;
using Extenso;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Mvc;

public partial class ExportController<T> : InfernoController
     where T : class, IEntity
{
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
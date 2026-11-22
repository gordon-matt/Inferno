using System.ComponentModel.DataAnnotations;

namespace Inferno.Web.Models;

/// <summary>How <see cref="Mvc.ExportController{T}.Download"/> writes a list.</summary>
public class DownloadOptions
{
    /// <summary>The file format.</summary>
    public DownloadFileFormat FileFormat { get; set; }

    /// <summary>The separator used when <see cref="FileFormat"/> is <see cref="DownloadFileFormat.Delimited"/>. Defaults to comma.</summary>
    public DownloadFileDelimiter Delimiter { get; set; } = DownloadFileDelimiter.Comma;

    /// <summary>Whether every delimited field is wrapped in quotes. Defaults to <see langword="true"/>.</summary>
    public bool AlwaysEnquote { get; set; } = true;

    /// <summary>Whether the delimited file starts with a header row. Defaults to <see langword="true"/>.</summary>
    public bool OutputColumnNames { get; set; } = true;
}

/// <summary>The file format of an export.</summary>
public enum DownloadFileFormat : byte
{
    /// <summary>A text file separated by <see cref="DownloadFileDelimiter"/>.</summary>
    Delimited = 0,

    /// <summary>An Excel workbook.</summary>
    XLSX = 1
}

/// <summary>The separator of a delimited export.</summary>
public enum DownloadFileDelimiter : byte
{
    /// <summary>Comma. The file is served as CSV.</summary>
    [Display(Name = "Comma (,)")]
    Comma = 0,

    /// <summary>Tab. The file is served as tab-separated values.</summary>
    [Display(Name = "Tab")]
    Tab = 1,

    /// <summary>Vertical bar. The file is served as plain text.</summary>
    [Display(Name = "Vertical Bar (|)")]
    VerticalBar = 2,

    /// <summary>Semicolon. The file is served as plain text.</summary>
    [Display(Name = "Semicolon (;)")]
    Semicolon = 3
}
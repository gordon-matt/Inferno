using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using InfernoCMS.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace InfernoCMS.Controllers
{
    [Route("person")]
    public class PersonController : ExportController<Person>
    {
        private readonly ApplicationDbContext context;

        public PersonController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("export/csv")]
        public FileResult ExportToCsv()
        {
            return Download(ApplyQuery(context.People, Request.Query), new DownloadOptions
            {
                FileFormat = DownloadFileFormat.Delimited
            });
        }

        [HttpGet("export/excel")]
        public FileResult ExportToExcel()
        {
            return Download(ApplyQuery(context.People, Request.Query), new DownloadOptions
            {
                FileFormat = DownloadFileFormat.XLSX
            });
        }
    }
}
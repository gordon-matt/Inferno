﻿using Extenso.Data.Entity;
using Inferno.Tenants.Entities;
using Inferno.Web.Models;
using Inferno.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.Areas.Admin.Tenants.Controllers
{
    [Area(InfernoWebConstants.Areas.Tenants)]
    [Route("tenants")]
    public class TenantController : ExportController<Tenant>
    {
        private readonly IRepository<Tenant> repository;

        public TenantController(IRepository<Tenant> repository)
        {
            this.repository = repository;
        }

        [HttpGet("export/csv")]
        public FileResult ExportToCsv()
        {
            using var connection = repository.OpenConnection();
            return Download(ApplyQuery(connection.Query(), Request.Query), new DownloadOptions
            {
                FileFormat = DownloadFileFormat.Delimited
            });
        }

        [HttpGet("export/excel")]
        public FileResult ExportToExcel()
        {
            using var connection = repository.OpenConnection();
            return Download(ApplyQuery(connection.Query(), Request.Query), new DownloadOptions
            {
                FileFormat = DownloadFileFormat.XLSX
            });
        }
    }
}
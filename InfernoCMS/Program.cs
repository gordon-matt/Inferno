﻿// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using Dependo.Autofac;
using OfficeOpenXml;

namespace InfernoCMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseServiceProviderFactory(new DependableAutofacServiceProviderFactory());
    }
}
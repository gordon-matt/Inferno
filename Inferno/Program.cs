using Dependo.Autofac;
using OfficeOpenXml;

namespace Inferno
{
    public class Program
    {
        public const string ODataBaseUri = "https://localhost:7082/odata/";

        public static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseServiceProviderFactory(new DependableAutofacServiceProviderFactory());
        }
    }
}
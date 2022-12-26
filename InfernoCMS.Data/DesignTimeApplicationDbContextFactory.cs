using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfernoCMS.Data
{
    // This is just used for EF Migrations (Not for Production)
    public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Initial Catalog=Inferno;User=sa;Password=Admin@123;Trust Server Certificate=true;");
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
using Microsoft.EntityFrameworkCore;

namespace Inferno.Data.Entity
{
    public interface IDbSeeder
    {
        void Seed(DbContext context);

        int Order { get; }
    }
}
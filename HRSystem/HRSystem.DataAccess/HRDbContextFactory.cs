using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HRSystem.DataAccess
{
    public class HRDbContextFactory : IDesignTimeDbContextFactory<HRDbContext>
    {
        public HRDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HRDbContext>();

            // غيّر هنا من UseSqlServer لـ UseSqlite
            optionsBuilder.UseSqlite("Data Source=HRSystem.db");

            return new HRDbContext(optionsBuilder.Options);
        }
    }
}

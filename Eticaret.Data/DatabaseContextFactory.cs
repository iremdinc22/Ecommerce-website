using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Eticaret.Data
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            Console.WriteLine("✅ DatabaseContextFactory çalıştı");

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite("Data Source=VscodeEticaret.Db"); // dosyanın adını kullan!

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}



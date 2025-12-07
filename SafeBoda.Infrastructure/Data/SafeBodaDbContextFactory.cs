using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SafeBoda.Infrastructure.Data;

public class SafeBodaDbContextFactory : IDesignTimeDbContextFactory<SafeBodaDbContext>
{
    public SafeBodaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SafeBodaDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=SafeBodaDb;Username=postgres;Password=UhoRaho@842");

        return new SafeBodaDbContext(optionsBuilder.Options);
    }
}


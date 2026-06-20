using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string dbName = "")
    {
        var name    = string.IsNullOrEmpty(dbName) ? Guid.NewGuid().ToString() : dbName;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
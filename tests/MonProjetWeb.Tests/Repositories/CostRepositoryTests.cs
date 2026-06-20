using FluentAssertions;
using MonProjetWeb.Persistence.Repositories;
using MonProjetWeb.Tests.Helpers;

namespace MonProjetWeb.Tests.Repositories;

public class CostRepositoryTests
{
    [Fact]
    public async Task GetByPeriod_ShouldReturnRecordsInRange()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var user     = TestDataSeeder.CreateUser(db);
        var account  = TestDataSeeder.CreateGcpAccount(db, user.Id);
        TestDataSeeder.CreateCostRecords(db, account.Id, 5);

        var repo  = new CostRepository(db);
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end   = DateTime.UtcNow.AddDays(1);

        // Act
        var records = await repo.GetByPeriodAsync(account.Id, start, end);

        // Assert
        records.Should().HaveCount(5);
        records.Should().AllSatisfy(r => r.GcpAccountId.Should().Be(account.Id));
    }

    [Fact]
    public async Task GetTotalCost_ShouldSumAllAmounts()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var user     = TestDataSeeder.CreateUser(db);
        var account  = TestDataSeeder.CreateGcpAccount(db, user.Id);
        TestDataSeeder.CreateCostRecords(db, account.Id, 5);

        var repo  = new CostRepository(db);
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end   = DateTime.UtcNow.AddDays(1);

        // Act
        var total = await repo.GetTotalCostAsync(account.Id, start, end);

        // Assert
        // Les montants sont 100, 200, 300, 400, 500 = 1500
        total.Should().Be(1500m);
    }

    [Fact]
    public async Task GetTopServices_ShouldReturnLimitedAndOrdered()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var user     = TestDataSeeder.CreateUser(db);
        var account  = TestDataSeeder.CreateGcpAccount(db, user.Id);
        TestDataSeeder.CreateCostRecords(db, account.Id, 5);

        var repo = new CostRepository(db);

        // Act
        var top3 = await repo.GetTopServicesAsync(account.Id, 3);

        // Assert
        top3.Should().HaveCount(3);
        var list = top3.ToList();
        list[0].Amount.Should().BeGreaterThanOrEqualTo(list[1].Amount);
        list[1].Amount.Should().BeGreaterThanOrEqualTo(list[2].Amount);
    }

    [Fact]
    public async Task GetCostGroupedByService_ShouldGroupCorrectly()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var user     = TestDataSeeder.CreateUser(db);
        var account  = TestDataSeeder.CreateGcpAccount(db, user.Id);
        TestDataSeeder.CreateCostRecords(db, account.Id, 5);

        var repo  = new CostRepository(db);
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end   = DateTime.UtcNow.AddDays(1);

        // Act
        var grouped = await repo.GetCostGroupedByServiceAsync(account.Id, start, end);

        // Assert
        grouped.Should().HaveCount(5);
        grouped.Keys.Should().Contain("Compute Engine");
        grouped.Keys.Should().Contain("Cloud Storage");
    }
}
using FluentAssertions;
using Moq;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Services;
using MonProjetWeb.Persistence.Repositories;
using MonProjetWeb.Tests.Helpers;

namespace MonProjetWeb.Tests.Services;

public class BudgetServiceTests
{
    [Fact]
    public async Task CreateBudget_ShouldPersistToDatabase()
    {
        // Arrange
        using var db      = TestDbContextFactory.Create();
        var user          = TestDataSeeder.CreateUser(db);
        var account       = TestDataSeeder.CreateGcpAccount(db, user.Id);

        var budgetRepo  = new BudgetRepository(db);
        var costRepo    = new CostRepository(db);
        var gcpMock     = new Mock<IGoogleCloudService>();
        var service     = new BudgetService(budgetRepo, costRepo, gcpMock.Object);

        // Act
        var budget = await service.CreateBudgetAsync(
            account.Id, "Budget Prod", 5000m,
            DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 80m);

        // Assert
        budget.Should().NotBeNull();
        budget.Id.Should().BeGreaterThan(0);
        budget.Name.Should().Be("Budget Prod");
        budget.Amount.Should().Be(5000m);
        budget.GcpAccountId.Should().Be(account.Id);

        var fromDb = await budgetRepo.GetByIdAsync(budget.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBudget_ShouldChangeAmount()
    {
        // Arrange
        using var db    = TestDbContextFactory.Create();
        var user        = TestDataSeeder.CreateUser(db);
        var account     = TestDataSeeder.CreateGcpAccount(db, user.Id);
        var budget      = TestDataSeeder.CreateBudget(db, account.Id, 1000m);

        var budgetRepo  = new BudgetRepository(db);
        var costRepo    = new CostRepository(db);
        var gcpMock     = new Mock<IGoogleCloudService>();
        var service     = new BudgetService(budgetRepo, costRepo, gcpMock.Object);

        // Act
        var updated = await service.UpdateBudgetAsync(budget.Id, 2000m, 90m);

        // Assert
        updated.Amount.Should().Be(2000m);
        updated.AlertThreshold.Should().Be(90m);
    }

    [Fact]
    public async Task DeleteBudget_ShouldRemoveFromDatabase()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);
        var budget     = TestDataSeeder.CreateBudget(db, account.Id);

        var budgetRepo = new BudgetRepository(db);
        var costRepo   = new CostRepository(db);
        var gcpMock    = new Mock<IGoogleCloudService>();
        var service    = new BudgetService(budgetRepo, costRepo, gcpMock.Object);

        // Act
        await service.DeleteBudgetAsync(budget.Id);

        // Assert
        var fromDb = await budgetRepo.GetByIdAsync(budget.Id);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task GetExceededBudgets_ShouldReturnOnlyExceeded()
    {
        // Arrange
        using var db   = TestDbContextFactory.Create();
        var user       = TestDataSeeder.CreateUser(db);
        var account    = TestDataSeeder.CreateGcpAccount(db, user.Id);

        // Budget dépassé (85% > seuil 80%)
        TestDataSeeder.CreateBudget(db, account.Id, 1000m, 850m);
        // Budget normal (50%)
        TestDataSeeder.CreateBudget(db, account.Id, 1000m, 500m);

        var budgetRepo = new BudgetRepository(db);
        var costRepo   = new CostRepository(db);
        var gcpMock    = new Mock<IGoogleCloudService>();
        var service    = new BudgetService(budgetRepo, costRepo, gcpMock.Object);

        // Act
        var exceeded = await service.GetExceededBudgetsAsync(account.Id);

        // Assert
        exceeded.Should().HaveCount(1);
        exceeded.First().SpentAmount.Should().Be(850m);
    }
}